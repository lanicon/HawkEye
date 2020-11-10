using HawkEye.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkEye.Scanning
{
    internal class Scanners
    {
        private LoggingSection logging;
        private List<Scanner> scanners;

        public Scanners()
        {
            logging = new LoggingSection(this);
            scanners = new List<Scanner>();
            logging.Info("Initializing scanners");
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && p != typeof(Scanner) && typeof(Scanner).IsAssignableFrom(p))
                .Where(t => t.GetConstructors().All(c => c.GetParameters().Length == 0)))
            {
                Scanner scanner = (Scanner)Activator.CreateInstance(type);
                logging.Debug($"Created instance of {type.Name}");
                scanners.Add(scanner);
            }
        }

        public Scanner[] GetScanners(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"No file was found at the given path: {filename}");
            return scanners.Where(scanner => scanner.IsValidFor(filename)).ToArray();
        }
    }
}