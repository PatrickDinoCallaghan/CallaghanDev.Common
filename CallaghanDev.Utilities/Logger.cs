
namespace CallaghanDev.Utilities
{
    public interface ILogger
    {
        public void WriteLine(string message);
        public void Info(string message, ConsoleColor consoleColor = ConsoleColor.White, bool SendEmail = false);
        public void Warning(string message, bool SendEmail = false);
        public void Error(string message, bool SendEmail = false);
    }
    public class Logger: ILogger
    {
        EmailSender _email;
        string _outputDirectory;
        public Logger(string OutputDirectory, EmailSender emailSender)
        {
            _email = emailSender;
            _outputDirectory = OutputDirectory;
        }
        public Logger(string OutputDirectory = "/LOGS")
        {
            _outputDirectory = OutputDirectory;
        }

        public void Info(string message, ConsoleColor consoleColor = ConsoleColor.White, bool SendEmail = false)
        {
            WriteToConsole("INFO", message, consoleColor);

            if (SendEmail)
            {
                _email?.SendEmail("patrick@callaghandev.com", "Error Notification", message);
            }
        }
        public void WriteLine(string message) => Info(message);
        public void Warning(string message, bool SendEmail = false)
        {
            WriteToConsole("WARNING", message, ConsoleColor.Yellow);
        }
        public void Error(string message, bool SendEmail = false)
        {
            WriteToConsole("ERROR", message, ConsoleColor.Red);
            WriteToFile(message);
            if (SendEmail)
            {
                _email?.SendEmail("patrick@callaghandev.com", "Error Notification", message);
            }
        }
        private void WriteToFile(string logEntry)
        {
            try
            {
                // Ensure the logs directory exists
                string logDirectory = _outputDirectory;
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Define log file path with a timestamped filename (one log per day)
                string logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

                // Append log entry to file
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle any file writing errors (optional: log to console)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to write log to file: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.ResetColor();
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
                Console.ResetColor();
            }
        }

    }
}