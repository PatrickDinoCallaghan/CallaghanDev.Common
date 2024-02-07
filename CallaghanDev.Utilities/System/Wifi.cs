using System.Diagnostics;
using System.Reflection;

namespace CallaghanDev.Utilities.System_
{
    public static class Wifi
    {
        /// <summary>
        /// Shows all wifi details including password. 
        /// </summary>
        /// <param name="wifiname">Name of wifi</param>
        /// <returns></returns>
        public static string ConnectedWifiDetails(string wifiname)
        {
            // netsh wlan show profile name=* key=clear
            string argument = "wlan show profile name=\"" + wifiname + "\" key=clear";
            Process processWifi = new Process();
            processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processWifi.StartInfo.FileName = "netsh";
            processWifi.StartInfo.Arguments = argument;
            processWifi.StartInfo.UseShellExecute = false;
            processWifi.StartInfo.RedirectStandardError = true;
            processWifi.StartInfo.RedirectStandardInput = true;
            processWifi.StartInfo.RedirectStandardOutput = true;
            processWifi.StartInfo.CreateNoWindow = true;
            processWifi.Start();
            //* Read the output (or the error)
            string output = processWifi.StandardOutput.ReadToEnd();
            // Show output commands
            string err = processWifi.StandardError.ReadToEnd();
            // show error commands
            processWifi.WaitForExit();
            return output;
        }
    }

}
