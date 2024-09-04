using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;
using System.Xml.Linq;
using Sprache;

namespace ThriveCsvEdiIntegration
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Load environment variables from the specified .env file (paths.env)
                Env.Load("./paths.env");

                string inputFolder = Environment.GetEnvironmentVariable("INPUTPATH");

                string[] files = Directory.GetFiles(inputFolder, "*csv");

                foreach (string file in files)
                {
                    
                    List<string> orderReferences  = await ReadCsvCustomerOrderRef(file, null);
                    foreach (string orderReference in orderReferences.Distinct())
                    {
                        Console.WriteLine("----------------------------");
                        List<string> mainOrderData = await ReadCsvCustomerOrderRef(file, orderReference);
                        string result = string.Join(",", mainOrderData);
                        Console.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("XML file updated and saved as 'output.xml'.");

            Console.ReadLine();
        }

        // Get distinct Customer Order references
        static private async Task<List<string>> ReadCsvCustomerOrderRef(string file, string orderRef)
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
                    if (orderRef == null)
                    {
                        orderReference.Add(columns[0]);
                    }
                    
                    if (orderRef != null && columns[0] == orderRef)
                    {
                        return columns.ToList();
                    }
                }
            }
            //CreateXmlOrder(orderReference.Distinct().ToList());
            return orderReference;
        }

        static void CreateXmlOrder(List<string> orderReference)
        {
            foreach (var orderRef in orderReference)
            {
                DateTime currentTime = DateTime.Now;

                string testTest = "";

                // Load the XML file
                XDocument xmlDoc = XDocument.Load(@"C:\Users\Pavel.Makarov\OneDrive - Rhenus Logistics\Desktop\Thrive\testXml.xml");

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

                    //// Modify the value of the <CustomerOrderDate> element
                    //XElement xmlCustomerOrderDate = root.Element("CustomerOrderDate");
                    //if (xmlCustomerOrderDate != null)
                    //{
                    //    xmlCustomerOrderDate.Value = currentTime.ToString("yyyy-MM-dd");
                    //}

                    List<string> xmlSegments = XmlSegments();
                    foreach (var xmlSegment in xmlSegments)
                    {
                        XElement xmlSelectedSegment = root.Element(xmlSegment);
                        if (xmlSelectedSegment != null)
                        {
                            xmlSelectedSegment.Value = !string.IsNullOrEmpty(testTest) ? testTest : "";
                        }
                    }
                }

                // Save the modified XML back to a file
                xmlDoc.Save(@"C:\Users\Pavel.Makarov\OneDrive - Rhenus Logistics\Desktop\Thrive\outputTest.xml");

            }
        }

        private static List<string> XmlSegments()
        {
            return new List<string>
            {
                "Narrative",
                "CustomerOrderReference",
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
