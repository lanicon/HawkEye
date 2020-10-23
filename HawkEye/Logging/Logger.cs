using System;
using System.Collections.Generic;
using System.Linq;

namespace HawkEye.Logging
{
    internal class Logger
    {
        private static LoggingSection logging = new LoggingSection("Logger");
        private static List<LogLevel> Levels { get; } = new List<LogLevel>();

        static Logger()
        {
            foreach (LogLevel logLevel in Enum.GetValues(typeof(LogLevel)))
                Levels.Add(logLevel);
        }

        public static void Log(LogMessage logMessage)
        {
            if (logMessage.LoggingSection.Disposed)
            {
                logging.Warning($"Tried to log a {logMessage.LogLevel}-Message in LoggingSection {logMessage.LoggingSection.Name}, but it has already been disposed");
                return;
            }

            //TODO: Add file logging support

            if (Levels.Contains(logMessage.LogLevel))
                Console.WriteLine(logMessage);
        }

        public static bool IsEnabled(LogLevel logLevel) => Levels.Contains(logLevel);

        public static void SetEnabled(LogLevel logLevel, bool enabled)
        {
            if (enabled)
            {
                if (IsEnabled(logLevel))
                    return;
                Levels.Add(logLevel);
            }
            else
                Levels.Remove(logLevel);
        }
    }
}