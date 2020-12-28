using HawkEye.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkEye.Scanning
{
    public abstract class Scanner
    {
        protected LoggingSection logging;

        public Scanner()
        {
            logging = new LoggingSection(this);
        }

        public ScanResult Scan(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            LoggingSection log = logging.CreateChild(fileInfo.Name);
            log.Verbose($"Starting {GetType().Name} on {fileInfo.FullName}");
            DateTime timeStarted = DateTime.Now;
            string result = null;
            bool succeeded;
            Exception exception = null;
            try
            {
                result = DoScan(fileInfo.FullName, log);
                succeeded = true;
                log.Verbose("Scan was successfull");
            }
            catch (Exception e)
            {
                succeeded = false;
                exception = e;
                log.Warning($"Scan failed: {e.Message}{Environment.NewLine}{e.StackTrace}");
            }
            DateTime timeEnded = DateTime.Now;
            log.Dispose();
            return new ScanResult(result, fileInfo.FullName, this, timeStarted, timeEnded, succeeded, exception);
        }

        public async Task<ScanResult> ScanAsync(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            LoggingSection log = logging.CreateChild(fileInfo.Name);
            log.Verbose($"Starting {GetType().Name} on {fileInfo.FullName}");
            DateTime timeStarted = DateTime.Now;
            string result = null;
            bool succeeded;
            Exception exception = null;
            try
            {
                result = await DoScanAsync(fileInfo.FullName, log);
                succeeded = true;
                log.Verbose("Scan was successfull");
            }
            catch (Exception e)
            {
                succeeded = false;
                exception = e;
                log.Warning($"Scan failed: {e.Message}{Environment.NewLine}{e.StackTrace}");
            }
            DateTime timeEnded = DateTime.Now;
            log.Dispose();
            return new ScanResult(result, fileInfo.FullName, this, timeStarted, timeEnded, succeeded, exception);
        }

        public abstract bool IsValidFor(string filename);

        protected abstract string DoScan(string filename, LoggingSection log);

        protected abstract Task<string> DoScanAsync(string filename, LoggingSection log);
    }
}