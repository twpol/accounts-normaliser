using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Formats
{
    static class Csv
    {
        public static Model.Account Read(string file, IConfigurationSection config)
        {
            var account = new Model.Account(null, null, Model.AccountType.Unknown, null);

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                MissingFieldFound = null,
            };

            using (var stream = File.OpenRead(file))
            {
                using (var reader = new StreamReader(stream, true))
                {
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
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
                            var amount = ParseDecimal(GetValue(config["Amount"], csv));
                            var amountType = GetValue(config["AmountType"], csv) ?? "";

                            if (amountType == "") amountType = config["AmountTypeDefault"] ?? "";

                            if (
                                (deposit != 0 && (withdrawal != 0 || amount != 0 || amountType != "")) ||
                                (withdrawal != 0 && (amount != 0 || amountType != "")) ||
                                (amount != 0 && amountType != config["AmountTypeDeposit"] && amountType != config["AmountTypeWithdrawal"])
                            ) throw new InvalidDataException($"CSV line {csv.Context.Parser.Row} contains mixture of deposit {deposit}, withdrawl {withdrawal}, amount {amount}, amount type {amountType} values");

                            if (deposit != 0)
                                amount = deposit;
                            else if (withdrawal != 0)
                                amount = -withdrawal;
                            else if (amount != 0 && amountType == config["AmountTypeWithdrawal"])
                                amount *= -1;

                            account.Transactions.Add(new Model.Transaction(
                                DateTimeOffset.Parse(GetValue(config["Date"], csv)),
                                amount,
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