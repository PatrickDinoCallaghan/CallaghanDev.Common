using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities
{
    public class EmailSender
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587; // Port for TLS
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailSender(string senderEmail, string senderPassword)
        {
            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new ArgumentException("Sender email cannot be null or empty.", nameof(senderEmail));
            if (string.IsNullOrWhiteSpace(senderPassword))
                throw new ArgumentException("Sender password cannot be null or empty.", nameof(senderPassword));

            _senderEmail = senderEmail;
            _senderPassword = senderPassword;
        }

        public void SendEmail(string recipientEmail, string subject, string body, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(recipientEmail));
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be null or empty.", nameof(subject));
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Body cannot be null or empty.", nameof(body));

            try
            {
                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                    smtpClient.EnableSsl = true;

                    using (var mailMessage = new MailMessage(_senderEmail, recipientEmail))
                    {
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = isHtml;

                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }

    }
}