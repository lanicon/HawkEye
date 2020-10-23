using System;
using System.Collections.Generic;
using System.Linq;

namespace HawkEye.Logging
{
    internal class Logger
    {
        private static LoggingSection logging = new LoggingSection("Logger");
        private static List<LogLevel> EnabledLevels { get; } = new List<LogLevel>((LogLevel[])Enum.GetValues(typeof(LogLevel)));

        public static void Log(LogMessage logMessage)
        {
            if (logMessage.LoggingSection.Disposed)
            {
                logging.Warning($"Tried to log a {logMessage.LogLevel}-Message in LoggingSection {logMessage.LoggingSection.Name}, but it has already been disposed");
                return;
            }

            //TODO: Add file logging support

            if (EnabledLevels.Contains(logMessage.LogLevel))
                Console.WriteLine(logMessage);
        }

        public static bool IsEnabled(LogLevel logLevel) => EnabledLevels.Contains(logLevel);

        public static void SetEnabled(LogLevel logLevel, bool enabled)
        {
            if (enabled)
            {
                if (IsEnabled(logLevel))
                    return;
                EnabledLevels.Add(logLevel);
            }
            else
                EnabledLevels.Remove(logLevel);
        }
    }
}