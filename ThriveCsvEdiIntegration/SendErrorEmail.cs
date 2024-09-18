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
        static string _app;
        static string _host;
        static int _port;
        static string _emails;

        // Asynchronously sends an email with a specified message
        public async Task SendEmail(string message)
        {
            // Reads SMTP configuration data from the "smtp.json" file into a dynamic array
            dynamic dataArray = await ReadJson("smtp.json");

            // smtp.json should hold smtp data only for one host. 
            foreach(var item in dataArray)
            {
                _app = item.app;
                _host = item.host;
                _port = item.port;
                _emails = item.emails;
            }

            // Iterates through each item in the dynamic dataArray (assumes multiple configurations or email settings)
            List<string> emailRecipients = GetEmailRecipients(_emails); ;

            // If there are email recipients available, proceed to send the email
            if (emailRecipients.Count() > 0)
            {
                // Sends the email using SMTP details and the recipients
                Send(message, emailRecipients);
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
        private void Send(string message, List<string> emails)
        {
            // Initializes a new SMTP client with the specified host and port
            var smtpClient = new SmtpClient(_host)
            {
                Port = _port,
                EnableSsl = true,  // Enables SSL for secure email sending
            };

            // Retrieves the machine name (for logging or subject line)
            string serverName = Environment.MachineName;
            string appRootFolder = AppDomain.CurrentDomain.BaseDirectory;

            // Creates a new email message
            var emailMessage = new MailMessage
            {
                From = new MailAddress($"{_app}_EDI_Manager@rhenus.com"),  // Sets the sender email address
                Subject = $"{_app} {serverName} Error",  // Sets the subject, including the server name
                Body = $"{message} \n {appRootFolder}",                   // Sets the email body (the message content)
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
                Write_Log("Error Email Was Sent!");
            }
            catch (Exception ex)
            {
                // Logs any errors that occur during the email sending process
                Write_Log($"Error class - SendErrorEmail / Send: {ex.Message}");
            }
        }
    }
}
