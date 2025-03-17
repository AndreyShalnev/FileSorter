using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ExternalSorting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build()
                .Get<Config>();

            Console.WriteLine($"Generate file with {config.NumberOfLines} lines");

            var generator = new InputGenerator(config);
            await generator.GenerateDataToFileAsync(config.InputFile);
            
            Console.WriteLine($"Data generation complete. {config.InputFile}");
        }
    }
}
