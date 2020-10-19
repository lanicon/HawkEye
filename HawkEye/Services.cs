using HawkEye.Commands;
using HawkEye.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HawkEye
{
    internal class Services
    {
        private static bool initiated = false;
        private static LoggingSection logging;
        private static List<IDisposable> services;

        public static CommandHandler CommandHandler { get; private set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initiate()
        {
            if (!initiated)
            {
                logging = new LoggingSection("Services");

                logging.Info("Initiating CommandHandler");
                CommandHandler = new CommandHandler();

                services = new List<IDisposable>() {
                    CommandHandler
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

                for (int i = services.Count - 1; i >= 0; i--)
                    Disable(services[i]);
            }
        }

        public static void Register(IDisposable service) => services.Add(service);

        public static void Disable(IDisposable service)
        {
            try
            {
                service.Dispose();
                services.Remove(service);
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