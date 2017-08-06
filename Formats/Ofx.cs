using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Formats
{
    static class Ofx
    {
        static Dictionary<Model.AccountType, string> OfxAccountTypes = new Dictionary<Model.AccountType, string> {
            { Model.AccountType.Unknown, "UNKNOWN" },
            { Model.AccountType.Checking, "CHECKING" },
            { Model.AccountType.Savings, "SAVINGS" },
            { Model.AccountType.MoneyMarket, "MONEYMRKT" },
            { Model.AccountType.CreditLine, "CREDITLINE" },
        };

        public static void Write(Model.Account account, string file, IConfigurationSection config)
        {
            var output = new XDocument(
                new XProcessingInstruction("OFX", "OFXHEADER=\"200\" VERSION=\"220\" SECURITY=\"NONE\" OLDFILEUID=\"NONE\" NEWFILEUID=\"NONE\""),
                new XElement("OFX",
                    new XElement("BANKMSGSRSV1",
                        new XElement("STMTTRNRS",
                            new XElement("STMTRS",
                                new XElement("CURDEF", account.Currency),
                                new XElement("BANKACCTFROM",
                                    new XElement("BANKID", account.BankID),
                                    new XElement("ACCTID", account.AccountID),
                                    new XElement("ACCTTYPE", OfxAccountTypes[account.AccountType])
                                ),
                                new XElement("BANKTRANLIST",
                                    from transaction in account.Transactions
                                    select new XElement("STMTTRN",
                                        new XElement("DTPOSTED", transaction.DatePosted.ToString("yyyyMMddHHmmss")),
                                        new XElement("TRNAMT", transaction.Amount),
                                        new XElement("NAME", transaction.Name),
                                        new XElement("MEMO", transaction.Memo)
                                    )
                                )
                            )
                        )
                    )
                )
            );

            using (var stream = File.OpenWrite(file))
            {
                output.Save(stream);
            }
        }
    }
}