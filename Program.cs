using System;
using CLP = CommandLineParser;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Accounts_Normaliser
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new CLP.Arguments.FileArgument('c', "config")
            {
                DefaultValue = new FileInfo("config.json")
            };

            var commandLineParser = new CLP.CommandLineParser()
            {
                Arguments = {
                    config,
                }
            };

            try
            {
                commandLineParser.ParseCommandLine(args);

                Main(new ConfigurationBuilder()
                    .AddJsonFile(config.Value.FullName, true)
                    .Build());
            }
            catch (CLP.Exceptions.CommandLineException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Main(IConfigurationRoot config)
        {
            foreach (var account in config.GetSection("Accounts").GetChildren())
                ProcessAccount(account);
        }

        private static void ProcessAccount(IConfigurationSection account)
        {
            Console.WriteLine($"Processing {account.Key}...");
            Console.WriteLine($"Done");
        }
    }
}
