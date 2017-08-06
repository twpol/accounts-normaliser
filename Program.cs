﻿using CLP = CommandLineParser;
using Microsoft.Extensions.Configuration;
using System;
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

        static void ProcessAccount(IConfigurationSection account)
        {
            Console.WriteLine($"Processing {account.Key}...");
            try {
                if (!Directory.Exists(account["Source"]))
                    throw new DirectoryNotFoundException($"Source directory {account["Source"]} does not exist");

                if (!Directory.Exists(account["Target"]))
                    throw new DirectoryNotFoundException($"Target directory {account["Target"]} does not exist");

                foreach (var file in Directory.GetFiles(account["Source"]))
                {
                    Console.WriteLine($"  Processing {Path.GetFileName(file)}...");
                    var data = ReadData(file, account.GetSection("SourceFormat"));
                    WriteData(data, Path.Combine(account["Target"], Path.GetFileNameWithoutExtension(file)), account.GetSection("TargetFormat"));
                }
            } catch (Exception error) {
                Console.WriteLine($"Failed with error: {error}");
                return;
            }
            Console.WriteLine("Done");
        }

        static Model.Account ReadData(string file, IConfigurationSection config)
        {
            switch (config["Type"])
            {
                default:
                    throw new NotImplementedException($"Source format {config["Type"]} is not supported");
            }
        }

        static void WriteData(Model.Account account, string file, IConfigurationSection config)
        {
            switch (config["Type"])
            {
                default:
                    throw new NotImplementedException($"Target format {config["Type"]} is not supported");
            }
        }
    }
}
