using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;

namespace CallaghanDev.Utilities.System_
{
    public class WindowsApplicationLauncherAsUser
    {
        ProcessStartInfo startInfo;
        public WindowsApplicationLauncherAsUser(string ProcessPath, string Domain, string Username, string Password)
        {
            startInfo = new ProcessStartInfo();
            startInfo.FileName = ProcessPath;

            // Set the start info properties for running as a different user
            startInfo.UseShellExecute = false;
            startInfo.Domain = Domain;
            startInfo.UserName = Username;
            SecureString securePassword = new SecureString();
            foreach (char c in Password)
            {
                securePassword.AppendChar(c);
            }
            startInfo.Password = securePassword;        
        }
        public void Start()
        {
            try
            {
                Process process = Process.Start(startInfo);

                Console.WriteLine("Process started successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
