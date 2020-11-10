using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HawkEye.Scanning
{
    public class ScanResult
    {
        public string Result { get; private set; }
        public object Target { get; private set; }
        public Scanner Scanner { get; private set; }
        public DateTime BeginOfScan { get; private set; }
        public DateTime EndOfScan { get; private set; }
        public bool Succeeded { get; private set; }
        public bool Failed { get { return !Succeeded; } }
        public Exception Exception { get; private set; }

        public ScanResult(string result, object target, Scanner scanner, DateTime beginOfScan, DateTime endOfScan, bool succeeded = true, Exception exception = null)
        {
            Result = result;
            Target = target;
            Scanner = scanner;
            BeginOfScan = beginOfScan;
            EndOfScan = endOfScan;
            Succeeded = succeeded;
            Exception = exception;
        }

        public override string ToString()
        {
            return $"Target: {Target}{Environment.NewLine}" +
                $"Scanner used: {Scanner.GetType().Name}{Environment.NewLine}" +
                $"Begin of scan: {BeginOfScan}{Environment.NewLine}" +
                $"End of scan: {EndOfScan}{Environment.NewLine}" +
                $"Succeeded: {Succeeded}{Environment.NewLine}" +
                $"Exception: {(Exception == null ? "None" : Exception.Message)}{Environment.NewLine}" +
                $"Result: {Result}";
        }
    }
}