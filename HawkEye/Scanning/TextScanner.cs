using HawkEye.Logging;
using HawkEye.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HawkEye.Scanning
{
    internal class TextScanner : Scanner
    {
        public override bool IsValidFor(string filename)
        {
            string extension = new FileInfo(filename).Extension.ToLower();
            //If the file is explicitly a txt, log or ini file, we dont have to check it's content. Otherwise check for a non-binary content to support additional file types.
            return extension == ".txt" || extension == ".log" || extension == ".ini" || !FileUtils.HasBinaryContent(filename);
        }

        protected override string DoScan(string filename, LoggingSection log)
        {
            DateTime beginning = DateTime.Now;
            try
            {
                log.Debug($"Extracting text from file");
                string text = File.ReadAllText(filename);
                log.Debug("Text extracted");
                return text;
            }
            catch (Exception)
            {
                throw;
                //return new ScanResult(null, filename, this, beginning, DateTime.Now, false, e);
            }
        }

        protected override async Task<string> DoScanAsync(string filename, LoggingSection log)
        {
            DateTime beginning = DateTime.Now;
            try
            {
                string text;
                log.Debug($"Extracting text from file");
                using (StreamReader streamReader = File.OpenText(filename))
                {
                    text = await streamReader.ReadToEndAsync();
                    log.Debug("Text extracted");
                }
                return text;
            }
            catch (Exception)
            {
                throw;
                //return new ScanResult(null, filename, this, beginning, DateTime.Now, false, e);
            }
        }
    }
}