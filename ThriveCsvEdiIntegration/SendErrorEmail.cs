using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ThriveCsvEdiIntegration
{
    internal class SendErrorEmail : ReadJsonData
    {
        // Asynchronously sends an email with a specified message
        public async Task SendEmail(string message)
        {
            // Reads SMTP configuration data from the "smtp.json" file into a dynamic array
            dynamic dataArray = await ReadJson("smtp.json");

            // Iterates through each item in the dynamic dataArray (assumes multiple configurations or email settings)
            foreach (var item in dataArray)
            {
                // Retrieves the list of email recipients from a file specified in the "emails" field of dataArray
                List<string> emailRecipients = GetEmailRecipients((string)dataArray.emails);

                // If there are email recipients available, proceed to send the email
                if (emailRecipients.Count() > 0)
                {
                    // Sends the email using SMTP details and the recipients
                    Send((string)dataArray.app,           // App name
                         (string)dataArray.host,          // SMTP server host
                         (int)dataArray.port,             // SMTP server port
                         message,                         // The message to be sent in the email
                         emailRecipients);                // List of recipients
                }
            }
        }

        // Retrieves email recipients from the specified file
        private List<string> GetEmailRecipients(string emailRecipientsFile)
        {
            // Creates a list to hold the email addresses
            List<string> emailRecipients = new List<string>();

            try
            {
                // Constructs the file path for the email recipients file (relative to the application's base directory)
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emailRecipientsFile);

                // Checks if the email recipients file exists
                if (File.Exists(filePath))
                {
                    // Reads all lines from the file (each line is assumed to be an email address)
                    string[] emailLines = File.ReadAllLines(filePath);

                    // Adds the email addresses from the file to the list
                    emailRecipients.AddRange(emailLines);
                }
                else
                {
                    // Logs an error if the file is not found
                    Write_Log($"Error class - SendErrorEmail / GetEmailRecipients: Can't find Email Recipients file!");
                }
            }
            catch (Exception ex)
            {
                // Logs any exceptions that occur while trying to read the email recipients file
                Write_Log($"Error class - SendErrorEmail / GetEmailRecipients: {ex.Message}");
            }

            // Returns the list of email recipients (empty if the file doesn't exist or is empty)
            return emailRecipients;
        }

        // Sends an email to the specified recipients using the provided SMTP details
        private void Send(string app, string host, int port, string message, List<string> emails)
        {
            // Initializes a new SMTP client with the specified host and port
            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                EnableSsl = true,  // Enables SSL for secure email sending
            };

            // Retrieves the machine name (for logging or subject line)
            string serverName = Environment.MachineName;

            // Creates a new email message
            var emailMessage = new MailMessage
            {
                From = new MailAddress($"{app}_Manager@rhenus.com"),  // Sets the sender email address
                Subject = $"{serverName} Error",  // Sets the subject, including the server name
                Body = message,                   // Sets the email body (the message content)
                IsBodyHtml = false,               // Specifies that the body is plain text, not HTML
            };

            // Adds all email recipients to the "To" field of the email
            foreach (var email in emails)
            {
                emailMessage.To.Add(email);
            }

            try
            {
                // Sends the email using the configured SMTP client
                smtpClient.Send(emailMessage);

                // Logs a message indicating the email was successfully sent
                Write_Log("Error Emails Were Sent!");
            }
            catch (Exception ex)
            {
                // Logs any errors that occur during the email sending process
                Write_Log($"Error class - SendErrorEmail / Send: {ex.Message}");
            }
        }
    }
}
