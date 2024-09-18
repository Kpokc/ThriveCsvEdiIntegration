using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveCsvEdiIntegration
{
    internal class ProcessFilesData
    {
        public async Task ProcessCustomerFiles(dynamic item)
        {
            try
            {
                Dictionary<string, List<string>> referenceCollection = new Dictionary<string, List<string>>();
                // Get all CSV files from the input path specified in the item
                string[] files = Directory.GetFiles((string)item.inputPath, "*csv");

                // Check if any files were found
                if (files.Length > 0)
                {
                    // Loop through each CSV file
                    foreach (string file in files)
                    {
                        // Read all distinct order references from the CSV file
                        List<string> orderReferences = await ReadCsvCustomerOrderRef(file, null, null);

                        referenceCollection.Add(file, orderReferences.Distinct().ToList());
                    }

                    foreach (var entry in referenceCollection)
                    {
                        string file = entry.Key;

                        foreach (var orderReference in entry.Value)
                        {
                            // Read the main order data for the specific order reference
                            List<string> mainOrderData = await ReadCsvCustomerOrderRef(file, orderReference, null);

                            // Combine the main order data into a single string
                            string orderData = string.Join(",", mainOrderData);

                            // Read the order lines related to the current order reference
                            List<string> orderLines = await ReadCsvCustomerOrderRef(file, null, orderReference);

                            // Create an XML file for the current order with the given parameters
                            AdjustAndSaveXml.CreateXmlOrder(
                                Path.GetFileNameWithoutExtension(file),  // File name without the extension
                                orderData,  // Combined order data
                                orderLines,  // List of order lines
                                (string)item.outputPath,  // Output path for the XML file
                                (string)item.xmlSample    // XML sample file for structure
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Get distinct Customer Order references
        // Method to read a CSV file and get distinct Customer Order references
        static private async Task<List<string>> ReadCsvCustomerOrderRef(string file, string orderRef, string orderLines)
        {
            // List to store order references
            List<string> orderReference = new List<string>();

            // Open the CSV file for reading
            using (var reader = new StreamReader(file))
            {
                // Skip the header line
                await reader.ReadLineAsync();

                // Initialize a string to store each line
                string line;

                // Loop through each line of the CSV file
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // Split the line into columns by comma delimiter
                    var columns = line.Split(',');

                    // If both orderRef and orderLines are null, add the first column (order reference) to the list
                    if (orderRef == null && orderLines == null)
                    {
                        orderReference.Add(columns[0]);
                    }

                    // If orderRef is not null and matches the first column, return the entire row as a list
                    if (orderRef != null && columns[0] == orderRef && orderLines == null)
                    {
                        return columns.ToList();  // Return the entire row as the result
                    }

                    // If orderLines is not null and matches the first column, concatenate the second and third columns and add to the list
                    if (orderLines != null && columns[0] == orderLines)
                    {
                        string orderLineData = string.Join(",", columns[1], columns[2]);  // Combine second and third columns
                        orderReference.Add(orderLineData);  // Add to the order reference list
                    }
                }
            }

            // Return the list of distinct order references
            return orderReference;
        }
    }
}
