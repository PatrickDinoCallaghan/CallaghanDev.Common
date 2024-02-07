using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.System_
{
    public class FileDirectoryHelper
    {
        public static IEnumerable<string> GetFiles(string directoryPath, List<string> fileExtensions)
        {
            var rtnfiles = new List<string>();
            List<string> files = fileExtensions.SelectMany(r => Directory.EnumerateFiles(directoryPath, $"*.{r}", SearchOption.AllDirectories)).ToList();

            foreach (var file in files)
            {
                files.Add(file);
            }
            return files;
        }
        public static void CreateDirectory(string path, bool DeleteContents = false)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                if (DeleteContents)
                {
                    DeleteContentsIfFolderExists(path);
                }
            }
        }
        public static void DeleteContentsIfFolderExists(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);

                // Delete all files in the directory
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }

                // Delete all subdirectories
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    dir.Delete(true); // true parameter deletes the subdirectories recursively
                }
            }
        }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
