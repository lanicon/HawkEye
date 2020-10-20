using HawkEye.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace HawkEye.Scanning
{
    internal class TextScanner : IScanner
    {
        public bool IsValidFor(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"No file was found at the given path: {filename}");
            string extension = new FileInfo(filename).Extension.ToLower();
            //If the file is explicitly a txt, log or ini file, we dont have to check it's content. Otherwise check for a non-binary content to support additional file types.
            return extension == "txt" || extension == "log" || extension == "ini" || !FileUtils.HasBinaryContent(filename);
        }

        public string Scan(string filename) => File.ReadAllText(filename);
    }
}