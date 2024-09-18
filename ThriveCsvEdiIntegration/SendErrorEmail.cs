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
        public async Task SendEmail(string message)
        {
            dynamic dataArray = await ReadJson("smtp.json");

            foreach(var item in dataArray)
            {
                List<string> emailRecipients = GetEmailRecipients((string)dataArray.emails);

                if (emailRecipients.Count() > 0)
                {
                    Send((string)dataArray.app, 
                        (string)dataArray.host, 
                        (int)dataArray.port, 
                        message, 
                        emailRecipients);
                }
            }
        }

        private List<string> GetEmailRecipients(string emailRecipientsFile)
        {
            List<string> emailRecipients = new List<string>();
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emailRecipientsFile);

                if (File.Exists(filePath))
                {
                    string[] emailLines = File.ReadAllLines(filePath);
                    emailRecipients.AddRange(emailLines);
                }
                else
                {
                    Write_Log($"Error class - SendErrorEmail / GetEmailRecipients: Can't find Email Recipients file!");
                }
            }
            catch (Exception ex)
            {
                Write_Log($"Error class - SendErrorEmail / GetEmailRecipients: {ex.Message}");
            }

            return emailRecipients;
        }

        private void Send(string app, string host, int port, string message, List<string> emails)
        {
            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                EnableSsl = true,
            };

            string serverName = Environment.MachineName;
            var emailMessage = new MailMessage
            {
                From = new MailAddress($"{app}_Manager@rhenus.com"),
                Subject = $"{serverName} Error",
                Body = message,
                IsBodyHtml = false,
            };

            foreach ( var email in emails ) 
            { 
                emailMessage.To.Add( email );
            }

            try
            {
                smtpClient.Send( emailMessage );
                Write_Log("Error Emais Was Sent!");
            }
            catch ( Exception ex )
            {
                Write_Log($"Error class - SendErrorEmail / Send: {ex.Message}");
            }
        }
    }
}
