using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace CallaghanDev.Utilities.Web
{

    public class FireFoxCookies : IGetCookies
    {
        public void ExtractCookies(string outputPath="")
        {
            string machineName = System.Environment.MachineName;
            List<SeleniumCookieDTO> cookies = new List<SeleniumCookieDTO>();
 
            long SearchCount = 0; 
            SQLitePCL.Batteries.Init();
            List<string> CookieFilePaths = GetCookieFilePaths(machineName);

            foreach (string dbPath in CookieFilePaths)
            {
                using (var conn = new SqliteConnection($"Data Source={dbPath};"))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM moz_cookies", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cookies.Add(new SeleniumCookieDTO(
                                    reader["name"].ToString(),
                                    reader["value"].ToString(),
                                    reader["host"].ToString(),
                                    reader["path"].ToString(),
                                    Convert.ToBoolean(reader["isSecure"]),
                                    false, // isHttpOnly column is not available in Firefox's cookies.sqlite
                                    null,  // sameSite column is not available in Firefox's cookies.sqlite
                                    DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(reader["expiry"])).DateTime
                                ));
                            }
                        }
                    }
                }
                SearchCount = SearchCount + 1;
                ConsoleHelper.cnsl.DisplayProgressBar(SearchCount, CookieFilePaths.Count, "Converting Cookies to selenium cookies.");
            }

            File.WriteAllText(outputPath, JsonConvert.SerializeObject(cookies, Formatting.Indented));
        }
        private List<string> GetCookieFilePaths(string machineName)
        {
            List<string> cookiesFiles = new List<string>();

            if (File.Exists($"{machineName}_moz_cookiesPaths.json"))
            {
                bool ans = ConsoleHelper.cnsl.AskYesNoQuestion("Do you want to use file paths previously found on this computer?");
                if (ans)
                {
                    var json = File.ReadAllText($"{machineName}_moz_cookiesPaths.json");
                    cookiesFiles = JsonConvert.DeserializeObject<List<string>>(json);
                }
            }

            if (cookiesFiles.Count == 0)
            {
                ConsoleHelper.cnsl.StartSpinner("Getting cookies from local computer.");
                cookiesFiles = FindCookiesFiles();
                ConsoleHelper.cnsl.StopSpinner();
            }
            if (cookiesFiles.Count == 0)
            {
                return null;
            }
            else
            {
                var json = JsonConvert.SerializeObject(cookiesFiles, Formatting.Indented);
                File.WriteAllText($"{machineName}_moz_cookiesPaths.json", json);
            }
            return cookiesFiles;
        }
        private List<string> FindCookiesFiles()
        {
            string root = GetRootDirectory();
            ConcurrentBag<string> foundPaths = new ConcurrentBag<string>();

            RecursiveSearch(root, foundPaths);

            return foundPaths.ToList();
        }
        private void RecursiveSearch(string rootDir, ConcurrentBag<string> foundPaths)
        {
            if (string.IsNullOrEmpty(rootDir))
                return;

            try
            {
                // Use ConcurrentBag to allow safe concurrent access from multiple threads
                var directories = Directory.GetDirectories(rootDir);

                // Process the files in the current directory
                var files = Directory.GetFiles(rootDir, "cookies.sqlite");
                foreach (var file in files)
                {
                    foundPaths.Add(file);
                }

                // Parallelize the processing of subdirectories
                Parallel.ForEach(directories, (currentDir) =>
                {
                    // Recurse into subdirectories
                    RecursiveSearch(currentDir, foundPaths);
                });
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"No access to {rootDir}. Skipping...");
            }
            catch (Exception ex) // You might want to handle other exceptions as well
            {
                Console.WriteLine($"Error processing {rootDir}: {ex.Message}");
            }
        }
        private long CountAllDirectories(string root, List<string> SkippedDirectories, long count = 0)
        {
           
            try
            {
                // Increment the count for the current directory
                count++;

                Console.SetCursorPosition(0, Console.CursorTop);
                System.Console.Write($"Number of directories the program will scan: {count}     "); // Added spaces to ensure previous text gets overwritten if shorter

                // Get all directories under the current directory and count them recursively
                foreach (var directory in Directory.GetDirectories(root))
                {
                    count = CountAllDirectories(directory, SkippedDirectories, count);  // Notice the change here
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle permissions issues - might not have access to all folders
                SkippedDirectories.Add($"Skipped directory {root} due to insufficient permissions.");
            }

            return count;
        }
        private string GetRootDirectory()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return "C:\\"; // Typical Windows root, adjust if needed
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return "/"; // Root for Unix and macOS
            }
            else
            {
                throw new NotSupportedException("Operating System not supported.");
            }
        }
    }

}