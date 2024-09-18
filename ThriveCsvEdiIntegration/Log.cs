using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveCsvEdiIntegration
{
    internal class Log
    {
        public static void Write_Log(string logEntry)
        {
            // Get current month and year
            string currentMonthYear = DateTime.Now.ToString("yyyy-MM");

            // Construct the file name
            string logFileName = $"Logs_for_{currentMonthYear}.txt";

            // Specify the directory path for logs
            string logDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine directory path and file name
            string logFilePath = Path.Combine(logDirectory, logFileName);

            // Check if the log file exists
            if (!File.Exists(logFilePath))
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(logDirectory);

                // Create a new log file
                using (StreamWriter writer = File.CreateText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now} - Log file created.");
                }
            }

            // Append the log entry to the file
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                writer.WriteLine($"{DateTime.Now} - {logEntry}");
            }
        }
    }
}
