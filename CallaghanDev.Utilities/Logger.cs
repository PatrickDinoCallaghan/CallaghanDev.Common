using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FParsec.ErrorMessage;

namespace CallaghanDev.Utilities
{
    public interface ILogger
    {
        public void WriteLine(string message);
        public void Info(string message);
        public void Warning(string message);
        public void Error(string message, bool Email = false);
    }
    public class Logger: ILogger
    {
        EmailSender _email;
        public Logger(EmailSender emailSender)
        {
            _email = emailSender;
        }
        public Logger()
        {

        }

        public void Info(string message)
        {
            WriteToConsole("WARNING", message, ConsoleColor.White);
        }

        public void WriteLine(string message) => Info(message);
        public void Warning(string message)
        {
            WriteToConsole("WARNING", message, ConsoleColor.Yellow);
        }

        public void Error(string message, bool SendEmail = false)
        {
            WriteToConsole("ERROR", message, ConsoleColor.Red);
            if (SendEmail)
            {
                _email?.SendEmail("patrick@callaghandev.com", "Error Notification", message);
            }
        }

        private void WriteToConsole(string logType, string message, ConsoleColor color)
        {  
            // Save the current console color
            var originalColor = Console.ForegroundColor;

            try
            {
                // Set the desired console color
                Console.ForegroundColor = color;

                // Write the message
                Console.WriteLine($"[{DateTime.Now}] {logType}: {message}");
            }
            finally
            {
                // Restore the original color
                Console.ForegroundColor = originalColor;
            }
        }

    }
}