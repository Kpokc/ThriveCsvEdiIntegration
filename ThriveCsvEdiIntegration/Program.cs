using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;

namespace ThriveCsvEdiIntegration
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Load environment variables from the specified .env file (paths.env)
                Env.Load("./paths.env");

                string inputFolder = Environment.GetEnvironmentVariable("INPUTPATH");

                string[] files = Directory.GetFiles(inputFolder, "*csv");

                foreach (string file in files) 
                {
                    Console.WriteLine(file);
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
