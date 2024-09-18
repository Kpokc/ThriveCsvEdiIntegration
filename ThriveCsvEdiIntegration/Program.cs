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
    internal class Program
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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        // Check customer data actual
        static private async Task CheckCustomersData(dynamic dataArray)
        {
            try
            {
                // Loop through each item in the dynamic data array
                foreach (dynamic item in dataArray)
                {
                    Console.WriteLine(item.customer);
                    //Stop executing script if one of the customers folder or/and xml file doesn't exists
                    if (CheckIfPathExists(item) == true)
                    {
                        Console.WriteLine($"Start to proces files.");
                        await ProcessCustomerFiles(item);
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur and print the error message
                Console.WriteLine(ex.Message);
            }
        }

        static private async Task ProcessCustomerFiles(dynamic item)
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
                            // Print the current order reference to the console
                            Console.WriteLine(orderReference);

                            // Read the main order data for the specific order reference
                            List<string> mainOrderData = await ReadCsvCustomerOrderRef(file, orderReference, null);

                            // Combine the main order data into a single string
                            string orderData = string.Join(",", mainOrderData);

                            // Read the order lines related to the current order reference
                            List<string> orderLines = await ReadCsvCustomerOrderRef(file, null, orderReference);

                            // Create an XML file for the current order with the given parameters
                            CreateXmlOrder(
                                Path.GetFileNameWithoutExtension(file),  // File name without the extension
                                orderData,  // Combined order data
                                orderLines,  // List of order lines
                                (string)item.outputPath,  // Output path for the XML file
                                (string)item.xmlSample    // XML sample file for structure
                            );
                        }
                        Console.WriteLine("------------------");
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

        // Method to create an XML order file based on the given data and a sample XML template
        static void CreateXmlOrder(string fileName, string mainOrderData, List<string> orderLines, string outputFolder, string custXmlSampleFile)
        {
            // Get the current date and time
            DateTime currentTime = DateTime.Now;

            // Split the main order data into an array of strings
            var mainData = mainOrderData.Split(',');

            // Load the sample XML file (used as a template for the XML generation)
            XDocument xmlDoc = XDocument.Load(custXmlSampleFile);

            // Get the root element of the XML document
            XElement root = xmlDoc.Element("Message");

            if (root != null)
            {
                // Modify the value of the <MessageID> element to be a timestamp (hours, minutes, seconds, milliseconds)
                XElement xmlMessageId = root.Element("MessageID");
                if (xmlMessageId != null)
                {
                    xmlMessageId.Value = currentTime.ToString("HHmmssfff");
                }

                // Modify the value of the <MessageCreatedDate> element to be today's date (YYYY-MM-DD)
                XElement xmlMessageCreatedDate = root.Element("MessageCreatedDate");
                if (xmlMessageCreatedDate != null)
                {
                    xmlMessageCreatedDate.Value = currentTime.ToString("yyyy-MM-dd");
                }

                // Modify the value of the <MessageCreatedTime> element to be the current time (HH:mm:ss)
                XElement xmlMessageCreatedTime = root.Element("MessageCreatedTime");
                if (xmlMessageCreatedTime != null)
                {
                    xmlMessageCreatedTime.Value = currentTime.ToString("HH:mm:ss");
                }

                // Modify the value of the <CustomerOrderReference> element to be the first value in the main order data
                XElement xmlCustomerOrderReference = root.Element("Order").Element("CustomerOrderReference");
                if (xmlCustomerOrderReference != null)
                {
                    xmlCustomerOrderReference.Value = mainData[0].ToString();
                }

                // Process additional segments (e.g., other columns of the main order data)
                int columnCounter = 3;  // Start from the 4th column (index 3)
                List<string> xmlSegments = XmlSegments();  // Get predefined XML segments

                foreach (var xmlSegment in xmlSegments)
                {
                    // Set values for the corresponding XML segments in the order section
                    XElement xmlSelectedSegment = root.Element("Order").Element(xmlSegment);
                    if (xmlSelectedSegment != null)
                    {
                        // Assign the value from the corresponding column in the main order data, or leave empty if not present
                        xmlSelectedSegment.Value = !string.IsNullOrEmpty(mainData[columnCounter]) ? mainData[columnCounter] : "";
                    }
                    columnCounter++;  // Move to the next column
                }

                // Get the <Lines> element where individual order lines will be added
                XElement orderElement = root.Element("Order").Element("Lines");

                if (orderElement != null)
                {
                    // Process each order line from the orderLines list
                    foreach (var lineData in orderLines)
                    {
                        // Check the quantity from the line data (second value in the comma-separated line)
                        string qtyToCheck = lineData.Split(',')[1];

                        // Only add the order line if the quantity is not zero
                        if (int.Parse(qtyToCheck) != 0)
                        {
                            // Create a new <Line> element with <StockCode> and <FullUnitQuantity> as sub-elements
                            XElement orderLine = new XElement("Line",
                                new XElement("StockCode", lineData.Split(',')[0]),  // Stock code (first value)
                                new XElement("FullUnitQuantity", lineData.Split(',')[1])  // Quantity (second value)
                            );

                            // Add the new <Line> element to the <Lines> section
                            orderElement.Add(orderLine);
                        }
                    }
                }
            }

            // Check if the <Line> element was added successfully to the XML document
            XElement LineElement = root.Element("Order").Element("Lines").Element("Line");

            if (LineElement != null)
            {
                // Create the output file name using the current time and the first value in the main data
                string fileToSave = Path.Combine(outputFolder, $"{fileName}_{currentTime.ToString("HHmmssfff")}_{mainData[0]}.xml");

                // Save the modified XML document to the output folder
                xmlDoc.Save(fileToSave);
            }
        }

        // Optional segments to be updated
        private static List<string> XmlSegments()
        {
            return new List<string>
            {
                "Narrative",
                "CustomerOrderDate",
                "OrderPartyReference",
                "OrderPartyAccountCode",
                "OrderPartyName",
                "OrderPartyAddressStreet",
                "OrderPartyAddressCity",
                "OrderPartyAddressCounty",
                "OrderPartyAddressPostCode",
                "OrderPartyAddressCountryISOCode",
                "OrderPartyInstructions",
                "OrderPartyContactName",
                "OrderPartyPhone",
                "OrderPartyMobile",
                "OrderPartyEmailAddress",
                "OrderPartyFax",
                "RequiredDeliveryDate",
                "RequiredDeliveryTime",
                "RequiredDeliveryEndTime"
            };
        }
    }
}
