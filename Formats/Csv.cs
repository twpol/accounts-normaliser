using System;
using System.IO;
using CsvHelper;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Formats
{
    static class Csv
    {
        public static Model.Account Read(string file, IConfigurationSection config)
        {
            var account = new Model.Account(null, null, Model.AccountType.Unknown, null);

            var csv = new CsvReader(new StreamReader(File.OpenRead(file), true));
            while (csv.Read())
            {
                if (csv.GetField(0) == csv.FieldHeaders[0])
                    continue;

                if (account.BankID == null & account.AccountID == null)
                {
                    account = new Model.Account(
                        GetValue(config["BankID"], csv),
                        GetValue(config["AccountID"], csv),
                        (Model.AccountType)Enum.Parse(typeof(Model.AccountType), GetValue(config["AccountType"], csv)),
                        GetValue(config["Currency"], csv)
                    );
                }

                var deposit = ParseDecimal(GetValue(config["Deposit"], csv));
                var withdrawal = ParseDecimal(GetValue(config["Withdrawal"], csv));

                if (deposit != 0 && withdrawal != 0)
                    throw new InvalidDataException($"CSV line {csv.Row} contains non-zero deposit {deposit} and non-zero withdrawal {withdrawal} values");

                account.Transactions.Add(new Model.Transaction(
                    DateTimeOffset.Parse(GetValue(config["Date"], csv)),
                    deposit - withdrawal,
                    GetValue(config["Description"], csv),
                    GetValue(config["Memo"], csv)
                ));
            }

            return account;
        }

        static string GetValue(string configValue, CsvReader csv)
        {
            if (configValue == null)
                return "";

            if (configValue.StartsWith("$column$"))
                return csv.GetField<string>(configValue.Substring(8));

            return configValue;
        }

        static decimal ParseDecimal(string value)
        {
            if (value == "")
                return 0;

            return decimal.Parse(value);
        }
    }
}