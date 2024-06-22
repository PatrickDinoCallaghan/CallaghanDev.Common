using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Web
{
    public class w3mCookies : IGetCookies
    {
        public void ExtractCookies(string outputPath = "")
        {
            // Path to the w3m cookie file                
            //CookiePath example : "/path/to/your/.w3m/cookie";
            var cookies = new List<SeleniumCookieDTO>();

            long SearchCount = 0;

            List<string> CookieFilePaths = GetW3mCookieFilePaths();

            foreach (string CookiePath in CookieFilePaths)
            {

                // Read the w3m cookie file
                if (File.Exists(CookiePath))
                {
                    var lines = File.ReadAllLines(CookiePath);

                    foreach (var line in lines)
                    {
                        // Parse each line based on w3m cookie file format
                        // Assuming format is: domain\ttailmatch\tpath\tsecure\texpiration\tname\tvalue
                        var parts = line.Split('\t');
                        if (parts.Length >= 7)
                        {
                            var expiration = long.Parse(parts[4]);

                            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiration).DateTime;

                            cookies.Add(new SeleniumCookieDTO(
                                        parts[5],
                                        parts[6],
                                        parts[0],
                                        parts[2],
                                        parts[3] == "TRUE",
                                        false, // isHttpOnly column is not available in Firefox's cookies.sqlite
                                        null,  // sameSite column is not available in Firefox's cookies.sqlite
                                        expiryDate
                                   ));
                        }
                    }
                    SearchCount = SearchCount + 1;
                    ConsoleHelper.cnsl.DisplayProgressBar(SearchCount, CookieFilePaths.Count(), "Converting Cookies to selenium cookies.");
                }
            }


            string OutFullPath = Path.Combine(outputPath, "cookies.json");
            File.WriteAllText(OutFullPath, JsonConvert.SerializeObject(cookies, Formatting.Indented));
        }

        private List<string> GetW3mCookieFilePaths()
        {
            string machineName = System.Environment.MachineName;
            List<string> cookiesFiles = new List<string>();

            if (File.Exists($"{machineName}_w3m_cookiesPaths.json"))
            {
                bool ans = ConsoleHelper.cnsl.AskYesNoQuestion("Do you want to use file paths previously found on this computer for w3m cookies?");
                if (ans)
                {
                    var json = File.ReadAllText($"{machineName}_w3m_cookiesPaths.json");
                    cookiesFiles = JsonConvert.DeserializeObject<List<string>>(json);
                }
            }

            if (cookiesFiles.Count == 0)
            {
                ConsoleHelper.cnsl.StartSpinner("Getting w3m cookies from local computer.");
                cookiesFiles = FindW3mCookiesFiles();
                ConsoleHelper.cnsl.StopSpinner();
            }
            if (cookiesFiles.Count == 0)
            {
                return null;
            }
            else
            {
                var json = JsonConvert.SerializeObject(cookiesFiles, Formatting.Indented);
                File.WriteAllText($"{machineName}_w3m_cookiesPaths.json", json);
            }
            return cookiesFiles;
        }

        private List<string> FindW3mCookiesFiles()
        {
            // Assuming the typical location of w3m cookies might be in user's home directory
            // This method should be implemented to search for w3m cookie files in likely locations
            List<string> foundFiles = new List<string>();
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string w3mCookiesPath = Path.Combine(homePath, ".w3m", "cookie");
            if (File.Exists(w3mCookiesPath))
            {
                foundFiles.Add(w3mCookiesPath);
            }
            // Implement additional search logic if necessary
            return foundFiles;
        }
    }
}
