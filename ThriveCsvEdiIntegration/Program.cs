using DotNetEnv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace ThriveCsvEdiIntegration
{
    internal class Program : Log
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Read Json data
                string jsonData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customers.json");
                // Open the JSON file for reading using StreamReader
                using (StreamReader r = new StreamReader(jsonData))
                {
                    // Asynchronously read the entire content of the JSON file
                    string json = await r.ReadToEndAsync();

                    // Deserialize the JSON data into a dynamic object (dataArray)
                    dynamic dataArray = JsonConvert.DeserializeObject(json);

                    // Start processing the data by calling StartToCheckInputFolder method with the deserialized data
                    CheckCustomersData(dataArray);
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur and print the error message
                //Console.WriteLine(ex.Message);
                Write_Log($"Error class - Main: {ex.Message}");
            }
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
