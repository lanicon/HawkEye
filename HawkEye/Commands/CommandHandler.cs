using HawkEye.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HawkEye.Commands
{
    public class CommandHandler : IDisposable
    {
        private LoggingSection logging;
        private List<ICommand> commands;
        private bool hasControl;

        public CommandHandler()
        {
            logging = new LoggingSection(this);
            commands = new List<ICommand>();
            hasControl = false;

            logging.Info("Initializing commands");
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && typeof(ICommand).IsAssignableFrom(p))
                .Where(t => t.GetConstructors().All(c => c.GetParameters().Length == 0)))
            {
                ICommand cmd = (ICommand)Activator.CreateInstance(type);
                logging.Info($"Initialized command {cmd.Name}");
                commands.Add(cmd);
            }
            logging.Info($"Initialized {commands.Count} commands");
        }

        public void HandleInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            input = input.Trim();

            string[] split = input.Split(' ');
            string commandName = split[0];

            ICommand cmd = commands.FirstOrDefault(c => c.Name.ToLower() == commandName.ToLower() || c.Alias.Any(a => a.ToLower() == commandName.ToLower()));

            if (cmd != null)
            {
                string[] args = new string[split.Length - 1];
                for (int i = 1; i < split.Length; i++)
                    args[i - 1] = split[i];

                try
                {
                    cmd.OnTrigger(args);
                }
                catch (Exception e)
                {
                    logging.Error($"" +
                        $"Unhandled exception while running command {cmd.Name}: {e.Message}" +
                        $"\nSource: {(e.Source != null ? e.Source : "Unknown")}" +
                        $"\nStackTrace: {e.StackTrace}");
                    throw;
                }
            }
            else
            {
                logging.Info($"No command found for \"{commandName}\".");
            }
        }

        public void TakeControl()
        {
            if (!hasControl)
            {
                logging.Info($"Took control over Thread \"{Thread.CurrentThread.Name}\"");
                hasControl = true;
                while (Program.IsRunning)
                    HandleInput(Console.ReadLine());
                hasControl = false;
            }
        }

        public void Dispose()
        {
            logging.Dispose();
        }
    }
}