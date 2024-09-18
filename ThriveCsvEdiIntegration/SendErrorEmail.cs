using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveCsvEdiIntegration
{
    internal class SendErrorEmail : ReadJsonData
    {
        public async void SendEmail()
        {
            dynamic dataArray = await ReadJson("customers.json");

        }
    }
}
