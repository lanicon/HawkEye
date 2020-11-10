using System;
using System.Collections.Generic;

namespace HawkEye.Logging
{
    public class LoggingSection : IDisposable
    {
        public LoggingSection Parent { get; private set; }
        public bool HasParent { get { return Parent != null; } }

        public List<LoggingSection> Children { get; private set; }
        public bool HasChildren { get { return Children != null && Children.Count > 0; } }

        public List<LogMessage> Messages { get; private set; }

        public string Name { get; private set; }

        public string FullPath
        {
            get
            {
                string path = Name;
                LoggingSection currentSection = this;
                while (currentSection.HasParent)
                {
                    path = Parent.Name + "/" + path;
                    currentSection = Parent;
                }
                return path;
            }
        }

        public bool Disposed { get; private set; }

        public LoggingSection(object obj, LoggingSection parent = null) : this(obj.GetType().Name, parent)
        { }

        public LoggingSection(string name, LoggingSection parent = null)
        {
            Name = name;

            Parent = parent;
            if (parent != null)
                parent.Children.Add(this);

            Children = new List<LoggingSection>();
            Messages = new List<LogMessage>();

            Disposed = false;
        }

        public LoggingSection CreateChild(string name)
        {
            if (Disposed)
                return null;
            return new LoggingSection(name, this);
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            for (int i = Children.Count - 1; i >= 0; i--)
                Children[i].Dispose();

            if (HasParent)
                Parent.Children.Remove(this);

            Parent = null;
            Messages = null;

            Disposed = true;
        }

        //Wrappers
        public void Debug(string message) => Logger.Log(new LogMessage(this, LogLevel.Debug, message, DateTime.Now));

        public void Verbose(string message) => Logger.Log(new LogMessage(this, LogLevel.Verbose, message, DateTime.Now));

        public void Info(string message) => Logger.Log(new LogMessage(this, LogLevel.Info, message, DateTime.Now));

        public void Warning(string message) => Logger.Log(new LogMessage(this, LogLevel.Warning, message, DateTime.Now));

        public void Error(string message) => Logger.Log(new LogMessage(this, LogLevel.Error, message, DateTime.Now));

        public void Critical(string message) => Logger.Log(new LogMessage(this, LogLevel.Critical, message, DateTime.Now));

        public void Log(LogLevel logLevel, string message) => Logger.Log(new LogMessage(this, logLevel, message, DateTime.Now));
    }
}