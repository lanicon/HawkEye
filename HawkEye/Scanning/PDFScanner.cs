using HawkEye.Logging;
using HawkEye.Utils;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;

namespace HawkEye.Scanning
{
    internal class PDFScanner : Scanner
    {
        public override bool IsValidFor(string filename)
        {
            return new FileInfo(filename).Extension.ToLower() == ".pdf";
        }

        protected override string DoScan(string filename, LoggingSection log)
        {
            PdfDocument pdfDocument = PdfReader.Open(filename);
            StringBuilder stringBuilder = new StringBuilder();
            for (int pageIndex = 0; pageIndex < pdfDocument.PageCount; pageIndex++)
            {
                log.Verbose($"Scanning page {pageIndex + 1} of {pdfDocument.PageCount}");
                PdfPage pdfPage = pdfDocument.Pages[pageIndex];
                //Extract text from text elements
                stringBuilder.Append($"{ExtractTextFromPdfPage(pdfPage)}{Environment.NewLine}");

                //Extract text from image elements with Tesseract OCR - awesome! :)
                PdfDictionary resources = pdfPage.Elements.GetDictionary("/Resources");
                if (resources != null)
                {
                    PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                    if (xObjects != null)
                    {
                        ICollection<PdfItem> items = xObjects.Elements.Values;
                        foreach (PdfItem item in items)
                        {
                            PdfReference reference = item as PdfReference;
                            if (reference != null)
                            {
                                PdfDictionary xObject = reference.Value as PdfDictionary;
                                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                {
                                    Bitmap bitmap = PdfImageToBitmap(xObject);
                                    if (bitmap == null)
                                    {
                                        log.Error("Could not extract bitmap from PDF image element. Seems like the PDF image filter type is not supported. Skipping element!");
                                        continue;
                                    }
                                    log.Debug("Rotating image");
                                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    log.Debug("Upscaling image 2x");
                                    BitmapUtils.Scale(ref bitmap, 2);
                                    log.Debug("Grayscaling image");
                                    BitmapUtils.GrayscaleWithLockBits(bitmap);
                                    log.Debug("Denoising image");
                                    BitmapUtils.DenoiseWithLockBits(bitmap);
                                    log.Debug("Applying OCR on image");
                                    Pix pix = PixConverter.ToPix(bitmap);
                                    TesseractEngine tesseractEngine = Services.OCRProvider.AwaitResource();
                                    Page tesseractPage = tesseractEngine.Process(pix);
                                    try
                                    {
                                        string text = tesseractPage.GetText();
                                        log.Debug($"Text is {text.Length} characters long");
                                        if (!string.IsNullOrWhiteSpace(text) && text != "\n")
                                            stringBuilder.Append(text.Replace("\n", " "));
                                    }
                                    catch (InvalidOperationException e)
                                    {
                                        log.Error($"OCR failed on Page {pageIndex} of file {filename}:\n{e.StackTrace}");
                                    }
                                    Services.OCRProvider.Feed(tesseractEngine);
                                    pix.Dispose();
                                }
                            }
                        }
                    }
                }
                stringBuilder.Append("\n");
            }

            log.Debug("Trimming text");
            string documentText = stringBuilder.ToString();
            documentText = documentText.Trim();
            while (documentText.Contains("  "))
                documentText = documentText.Replace("  ", " ");
            while (documentText.Contains("\n\n"))
                documentText = documentText.Replace("\n\n", "\n");
            return stringBuilder.ToString();
        }

        protected override Task<string> DoScanAsync(string filename, LoggingSection log)
        {
            throw new NotImplementedException();
        }

        private static Bitmap PdfImageToBitmap(PdfDictionary image)
        {
            string filter = image.Elements.GetName(PdfDictionary.PdfStream.Keys.Filter);
            switch (filter)
            {
                case "/DCTDecode":
                    byte[] stream = image.Stream.Value;
                    return new Bitmap(new MemoryStream(stream));

                //Thanks to VacentViscera - https://forum.pdfsharp.net/viewtopic.php?f=8&t=3801
                case "/FlateDecode":
                    int width = image.Elements.GetInteger(PdfImage.Keys.Width);
                    int height = image.Elements.GetInteger(PdfImage.Keys.Height);
                    int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

                    FlateDecode flate = new FlateDecode();
                    byte[] imageData = flate.Decode(image.Stream.Value);

                    PixelFormat pixelFormat;

                    switch (bitsPerComponent)
                    {
                        case 1:
                            pixelFormat = PixelFormat.Format1bppIndexed;
                            break;

                        case 8:
                            pixelFormat = PixelFormat.Format8bppIndexed;
                            break;

                        case 24:
                            pixelFormat = PixelFormat.Format24bppRgb;
                            break;

                        default:
                            throw new Exception("Unknown pixel format: " + bitsPerComponent);
                    }

                    Bitmap bitmap = new Bitmap(width, height, pixelFormat);
                    var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
                    int length = (int)Math.Ceiling(width * bitsPerComponent / 8.0);

                    for (int i = 0; i < height; i++)
                        Marshal.Copy(imageData, i * length, new IntPtr(bmpData.Scan0.ToInt32() + i * bmpData.Stride), length);

                    bitmap.UnlockBits(bmpData);
                    return bitmap;

                default:
                    return null;
            }
        }

        private static IEnumerable<string> ExtractTextFromCObject(CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                        foreach (var txt in ExtractTextFromCObject(cOperand))
                            yield return txt;
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                    foreach (var txt in ExtractTextFromCObject(element))
                        yield return txt;
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return cString.Value;
            }
        }

        private static string ExtractTextFromPdfPage(PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = ExtractTextFromCObject(content);
            return string.Join("", text);
        }
    }
}