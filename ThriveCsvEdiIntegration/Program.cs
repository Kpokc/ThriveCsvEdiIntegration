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
            // Load environment variables from the specified .env file (paths.env)
            Env.Load("./paths.env");
            string jsonData = Environment.GetEnvironmentVariable("JSONDATA");

            try
            {
                using (StreamReader r = new StreamReader(jsonData))
                {
                    string json = await r.ReadToEndAsync();
                    dynamic dataArray = JsonConvert.DeserializeObject(json);
                    StartToCheckInputFolder(dataArray);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Console.WriteLine("XML file updated and saved as 'output.xml'.");

            Console.ReadLine();
        }

        static private void StartToCheckInputFolder(dynamic dataArray)
        {
            try
            {
                Task.Run(() => 
                {
                    foreach (dynamic item in dataArray)
                    {
                        string[] files = Directory.GetFiles((string)item.inputPath, "*csv");
                        if (files != null)
                        {
                            foreach (string file in files)
                            {
                                Console.WriteLine(file);
                            }
                        }
                    }
                });
                //// Load environment variables from the specified .env file (paths.env)
                //Env.Load("./paths.env");

                //string custXmlSampleFile = Environment.GetEnvironmentVariable("CUSTXMLSAMPLEFILE");
                //string inputFolder = Environment.GetEnvironmentVariable("INPUTPATH");
                //string outputFolder = Environment.GetEnvironmentVariable("OUTPUTPATH");

                

                //foreach (string file in files)
                //{

                //    List<string> orderReferences = await ReadCsvCustomerOrderRef(file, null, null);

                //    foreach (string orderReference in orderReferences.Distinct())
                //    {
                //        Console.WriteLine(orderReference);
                //        List<string> mainOrderData = await ReadCsvCustomerOrderRef(file, orderReference, null);
                //        string orderData = string.Join(",", mainOrderData);

                //        List<string> orderLines = await ReadCsvCustomerOrderRef(file, null, orderReference);
                //        foreach (string line in orderLines)
                //        {
                //            Console.WriteLine(line);
                //        }

                //        CreateXmlOrder(Path.GetFileNameWithoutExtension(file), orderData, orderLines, outputFolder, custXmlSampleFile);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Get distinct Customer Order references
        static private async Task<List<string>> ReadCsvCustomerOrderRef(string file, string orderRef, string orderLines)
        {
            List<string> orderReference = new List<string>();

            // Open the CSV file for reading and the output files for writing
            using (var reader = new StreamReader(file))
            {
                // Read and write the header line
                await reader.ReadLineAsync();

                // Process each line of the CSV file
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var columns = line.Split(',');

                    if (orderRef == null && orderLines == null)
                    {
                        orderReference.Add(columns[0]);
                    }
                    
                    if (orderRef != null && columns[0] == orderRef && orderLines == null)
                    {
                        return columns.ToList();
                    }

                    if (orderLines != null && columns[0] == orderLines)
                    {
                        string orderLineData = string.Join(",", columns[1], columns[2]);
                        orderReference.Add(orderLineData);
                    }
                }
            }
            //CreateXmlOrder(orderReference.Distinct().ToList());
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
