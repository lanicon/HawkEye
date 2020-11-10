using System;

namespace HawkEye.Logging
{
    public class LogMessage
    {
        public LogLevel LogLevel { get; private set; }
        public LoggingSection LoggingSection { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        public LogMessage(LoggingSection loggingSection, LogLevel logLevel, string message, DateTime timestamp)
        {
            LoggingSection = loggingSection;
            LogLevel = logLevel;
            Message = message;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"{Timestamp} - [{LogLevel}] - [{LoggingSection.FullPath}]: {Message}";
        }
    }
}