using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveCsvEdiIntegration
{
    internal class ReadJsonData : Log
    {
        public static async Task<dynamic> ReadJson(string fileName)
        {
            try
            {
                // Read Json data
                string jsonData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                // Open the JSON file for reading using StreamReader
                using (StreamReader r = new StreamReader(jsonData))
                {
                    // Asynchronously read the entire content of the JSON file
                    string json = await r.ReadToEndAsync();

                    // Deserialize the JSON data into a dynamic object (dataArray)
                    dynamic dataArray = JsonConvert.DeserializeObject(json);

                    return dataArray;
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur and print the error message
                //Console.WriteLine(ex.Message);
                Write_Log($"Error class - ReadJson: {ex.Message}");
                return null;
            }
        }
    }
}
