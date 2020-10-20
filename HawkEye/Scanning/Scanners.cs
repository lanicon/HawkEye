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
        private List<IScanner> scanners;

        public Scanners()
        {
            logging = new LoggingSection(this);
            scanners = new List<IScanner>();
            logging.Info("Initializing scanners");
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && typeof(IScanner).IsAssignableFrom(p))
                .Where(t => t.GetConstructors().All(c => c.GetParameters().Length == 0)))
            {
                IScanner scanner = (IScanner)Activator.CreateInstance(type);
                logging.Debug($"Created instance of {type.Name}");
                scanners.Add(scanner);
            }
        }

        public IScanner[] GetScanners(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"No file was found at the given path: {filename}");
            return scanners.Where(scanner => scanner.IsValidFor(filename)).ToArray();
        }
    }
}