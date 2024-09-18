using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ThriveCsvEdiIntegration
{
    internal class AdjustAndSaveXml : Log
    {
        // Method to create an XML order file based on the given data and a sample XML template
        static public void CreateXmlOrder(string fileName, string mainOrderData, List<string> orderLines, string outputFolder, string custXmlSampleFile)
        {
            try
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
            catch (Exception ex)
            {
                Write_Log($"Error class - AdjustAndSaveXml: {ex.Message}");
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
