using System;
using System.IO;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

namespace AddBorder
{
    class Program
    {
        static void Main(string[] args)
        {
            bool success = true;

            string basePath = Directory.GetParent(AppContext.BaseDirectory).FullName;
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", false);

            var config = builder.Build();

            if (args.Length > 0)
            {
                Console.WriteLine(string.Join(",", args));
                System.Console.WriteLine();
                success = AddThatBorder.ProvideBorder(args, config);
            }
            else
            {
                System.Console.WriteLine("No files to process. Exiting.");
            }

            if (!success)
            {
                Timer timer = new Timer();
                timer.Elapsed += (sender, obj) => { System.Environment.Exit(0); ; };
                timer.Interval = int.Parse(config["WaitInMilliseconds"]);
                timer.Start();

                Console.ReadKey();
            }
        }
    }
}
