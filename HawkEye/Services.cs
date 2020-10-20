using HawkEye.Commands;
using HawkEye.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Tesseract;

namespace HawkEye
{
    internal class Services
    {
        private static bool initiated = false;
        private static LoggingSection logging;
        private static List<IDisposable> servicesToBeDisposed;

        public static CommandHandler CommandHandler { get; private set; }
        public static TesseractEngine OCR { get; private set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initiate()
        {
            if (!initiated)
            {
                logging = new LoggingSection("Services");

                logging.Info("Initiating CommandHandler");
                CommandHandler = new CommandHandler();

                logging.Info("Initiating Tesseract OCR Engine");
                OCR = new TesseractEngine(@"./Tesseract/tessdata", "deu");
                logging.Debug($"Using Tesseract {OCR.Version}");

                servicesToBeDisposed = new List<IDisposable>() {
                    CommandHandler,
                    OCR
                };

                initiated = true;
                logging.Info("Services initiation complete");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Dispose()
        {
            if (initiated)
            {
                logging.Info("Disposing Services");

                for (int i = servicesToBeDisposed.Count - 1; i >= 0; i--)
                    Disable(servicesToBeDisposed[i]);
            }
        }

        public static void Register(IDisposable service) => servicesToBeDisposed.Add(service);

        public static void Disable(IDisposable service)
        {
            try
            {
                service.Dispose();
                servicesToBeDisposed.Remove(service);
                logging.Debug($"{service.GetType().Name} disposed");
            }
            catch (Exception e)
            {
                logging.Error($"" +
                $"Unhandled exception while disposing Service {service.GetType().Name}: {e.Message}" +
                $"\nSource: {(e.Source != null ? e.Source : "Unknown")}" +
                $"\nStackTrace: {e.StackTrace}");
                throw;
            }
        }
    }
}