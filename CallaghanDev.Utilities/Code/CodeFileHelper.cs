using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Code
{
    public class FileHelper
    {
        public StringBuilder ConvertCsFileToStringBuilder(string path)
        {
            StringBuilder sb = new StringBuilder();

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            return sb;
        }
        public void WriteStringToCsFile(string content, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                // Splitting the string on every ';' character
                var lines = content.Split(new[] { ';' }, StringSplitOptions.None);

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        sw.Write(line.Trim());
                        sw.WriteLine(";");  // Append the ';' at the end of the line
                    }
                }
            }
        }
    }
}
