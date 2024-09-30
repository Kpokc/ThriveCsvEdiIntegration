using DotNetEnv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace ThriveCsvEdiIntegration
{
    internal class Program : Log
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 5;

        static async Task Main(string[] args)
        {
            // Hide the console window
            IntPtr hWndConsole = GetConsoleWindow();

            if (hWndConsole != IntPtr.Zero)
            {
                ShowWindow(hWndConsole, SW_HIDE);
            }

            dynamic dataArray = await ReadJsonData.ReadJson("customers.json");

            // Start processing the data by calling StartToCheckInputFolder method with the deserialized data
            await CheckCustomersData(dataArray);
        }

        // Check customer data actual
        static private async Task CheckCustomersData(dynamic dataArray)
        {
            ProcessFilesData processFilesData = new ProcessFilesData();
            try
            {
                // Loop through each item in the dynamic data array
                foreach (dynamic item in dataArray)
                {
                    //Stop executing script if one of the customers folder or/and xml file doesn't exists
                    if (CheckIfPathExists(item) == true)
                    {
                        await processFilesData.ProcessCustomerFiles(item);
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur and print the error message
                //Console.WriteLine(ex.Message);
                Write_Log($"Error class - Main: {ex.Message}");
            }
        }

        static private bool CheckIfPathExists(dynamic item)
        {
            Console.WriteLine("Check paths are existing");
            try 
            {
                // Check if all necessary directories exist, return false if any do not
                if (!Directory.Exists((string)item.inputPath) ||
                    !Directory.Exists((string)item.outputPath) ||
                    !Directory.Exists((string)item.archivePath) ||
                    !File.Exists((string)item.xmlSample))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur and print the error message
                //Console.WriteLine(ex.Message);
                Write_Log($"Error class - Main: {ex.Message}");
            }
            return true;
        }
    }
}
