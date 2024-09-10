using DotNetEnv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                    StartToCheckInputFolder(dataArray);
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur and print the error message
                Console.WriteLine(ex.Message);
            }
        }

        static private async Task StartToCheckInputFolder(dynamic dataArray)
        {
            try
            {
                foreach (dynamic item in dataArray)
                {
                    string[] files = Directory.GetFiles((string)item.inputPath, "*csv");
                    if (files != null)
                    {
                        foreach (string file in files)
                        {
                            Console.WriteLine(file);
                            List<string> orderReferences = await ReadCsvCustomerOrderRef(file, null, null);

                            foreach (string orderReference in orderReferences.Distinct())
                            {
                                Console.WriteLine(orderReference);
                                List<string> mainOrderData = await ReadCsvCustomerOrderRef(file, orderReference, null);
                                string orderData = string.Join(",", mainOrderData);

                                List<string> orderLines = await ReadCsvCustomerOrderRef(file, null, orderReference);
                                foreach (string line in orderLines)
                                {
                                    Console.WriteLine(line);
                                }

                                CreateXmlOrder(Path.GetFileNameWithoutExtension(file), orderData, orderLines, (string)item.outputPath, (string)item.xmlSample);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        static void CreateXmlOrder(string fileName, string mainOrderData, List<string> orderLines, string outputFolder, string custXmlSampleFile)
        {
            DateTime currentTime = DateTime.Now;

            var mainData = mainOrderData.Split(',');

            // Load the XML file
            XDocument xmlDoc = XDocument.Load(custXmlSampleFile);

            // Replace the values of certain elements
            XElement root = xmlDoc.Element("Message");

            if (root != null)
            {
                // Modify the value of the <MessageID> element
                XElement xmlMessageId = root.Element("MessageID");
                if (xmlMessageId != null)
                {
                    xmlMessageId.Value = currentTime.ToString("HHmmssfff");
                }

                // Modify the value of the <MessageCreatedDate> element
                XElement xmlMessageCreatedDate = root.Element("MessageCreatedDate");
                if (xmlMessageCreatedDate != null)
                {
                    xmlMessageCreatedDate.Value = currentTime.ToString("yyyy-MM-dd");
                }

                // Modify the value of the <MessageCreatedTime> element
                XElement xmlMessageCreatedTime = root.Element("MessageCreatedTime");
                if (xmlMessageCreatedTime != null)
                {
                    xmlMessageCreatedTime.Value = currentTime.ToString("HH:mm:ss");
                }

                // Modify the value of the <CustomerOrderReference> element
                XElement xmlCustomerOrderReference = root.Element("Order").Element("CustomerOrderReference");
                if (xmlCustomerOrderReference != null)
                {
                    xmlCustomerOrderReference.Value = mainData[0].ToString();
                }

                int columnCounter = 3;
                List<string> xmlSegments = XmlSegments();
                foreach (var xmlSegment in xmlSegments)
                {
                    XElement xmlSelectedSegment = root.Element("Order").Element(xmlSegment);
                    if (xmlSelectedSegment != null)
                    {
                        xmlSelectedSegment.Value = !string.IsNullOrEmpty(mainData[columnCounter]) ? mainData[columnCounter] : "";
                    }
                    columnCounter++;
                }

                XElement orderElement = root.Element("Order").Element("Lines");

                if (orderElement != null)
                {
                    foreach (var lineData in orderLines)
                    {
                        string qtyToCheck = lineData.Split(',')[1];
                        if (int.Parse(qtyToCheck) != 0)
                        {
                            XElement orderLine = new XElement("Line",
                                new XElement("StockCode", lineData.Split(',')[0]),
                                new XElement("FullUnitQuantity", lineData.Split(',')[1])
                                );

                            orderElement.Add(orderLine);
                        }
                    }
                }
            }

            XElement LineElement = root.Element("Order").Element("Lines").Element("Line");

            if (LineElement != null)
            {
                string fileToSave = Path.Combine(outputFolder, $"{ fileName}_{ currentTime.ToString("HHmmssfff")}_{ mainData[0]}.xml");
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
