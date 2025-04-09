using System;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Formats
{
    static class Qif
    {
        public static Model.Account Read(string file, IConfigurationSection config)
        {
            var account = new Model.Account(null, null, Model.AccountType.Unknown, null);

            using (var stream = File.OpenRead(file))
            {
                using (var reader = new StreamReader(stream, true))
                {
                    var header = GetNextLine(reader);
                    switch (header)
                    {
                        case "!Type:Cash":
                        case "!Type:Bank":
                        case "!Type:CCard":
                            account = new Model.Account(config["BankID"], config["AccountID"], Model.AccountType.Checking, config["Currency"]);
                            break;
                        default:
                            throw new InvalidDataException($"QIF contains unknown header {header}");
                    }

                    var emptyDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    var date = emptyDate;
                    decimal amount = 0;
                    var description = "";
                    var memo = "";

                    while (true)
                    {
                        var line = GetNextLine(reader);
                        if (line == null)
                        {
                            if (date != emptyDate || amount != 0 || description != "" || memo != "")
                                throw new InvalidDataException($"QIF ended in the middle of a transaction entry (date = {date}, amount = {amount}, description = {description}, memo = {memo}");
                            break;
                        }

                        switch (line[0])
                        {
                            case 'D':
                                date = DateTimeOffset.Parse(line.Substring(1));
                                break;
                            case 'T':
                                amount = ParseDecimal(line.Substring(1));
                                break;
                            case 'P':
                                description = line.Substring(1);
                                break;
                            case 'M':
                                memo = line.Substring(1);
                                break;
                            case '^':
                                account.Transactions.Add(new Model.Transaction(
                                    date,
                                    amount,
                                    description,
                                    memo
                                ));
                                date = emptyDate;
                                amount = 0;
                                description = "";
                                memo = "";
                                break;
                        }
                    }
                }
            }

            return account;
        }

        static string GetNextLine(StreamReader reader)
        {
            var line = "";
            while (!reader.EndOfStream && line.Length == 0)
                line = reader.ReadLine().Trim();

            if (reader.EndOfStream && line.Length == 0)
                return null;

            return line;
        }

        static string NumericCharacters = "-0123456789.,";

        static decimal ParseDecimal(string value)
        {
            var buffer = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
                if (NumericCharacters.IndexOf(value[i]) >= 0)
                    buffer.Append(value[i]);

            if (value.Length == 0)
                return 0;

            return decimal.Parse(buffer.ToString());
        }
    }
}