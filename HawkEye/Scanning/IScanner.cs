namespace HawkEye.Scanning
{
    internal interface IScanner
    {
        bool IsValidFor(string filename);

        string Scan(string filename);
    }
}