using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkEye.Utils
{
    internal class FileUtils
    {
        private static char[] controlCharWhitelist = { '\r', '\n', '\t' };

        public static bool HasBinaryContent(string filename)
        {
            string content;
            using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
                content = reader.ReadToEnd();
            return content.Any(character => char.IsControl(character) && !controlCharWhitelist.Contains(character));
        }
    }
}