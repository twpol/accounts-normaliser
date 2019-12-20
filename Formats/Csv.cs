using System;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Formats
{
    static class Csv
    {
        public static Model.Account Read(string file, IConfigurationSection config)
        {
            var account = new Model.Account(null, null, Model.AccountType.Unknown, null);

            using (var stream = File.OpenRead(file))
            {
                using (var reader = new StreamReader(stream, true))
                {
                    using (var csv = new CsvReader(reader))
                    {
                        csv.Configuration.BadDataFound = null;
                        csv.Read();
                        csv.ReadHeader();
                        while (csv.Read())
                        {
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
                                throw new InvalidDataException($"CSV line {csv.Context.Row} contains non-zero deposit {deposit} and non-zero withdrawal {withdrawal} values");

                            account.Transactions.Add(new Model.Transaction(
                                DateTimeOffset.Parse(GetValue(config["Date"], csv)),
                                deposit - withdrawal,
                                GetValue(config["Description"], csv),
                                GetValue(config["Memo"], csv)
                            ));
                        }
                    }
                }
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