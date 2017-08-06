using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Model
{
    class Account
    {
        public string BankID { get; }
        public string AccountID { get; }
        public AccountType AccountType { get; }
        public List<Transaction> Transactions { get; }

        public Account(string bankID, string accountID, AccountType accountType)
        {
            BankID = bankID;
            AccountID = accountID;
            AccountType = accountType;
            Transactions = new List<Transaction>();
        }
    }

    enum AccountType
    {
        Unknown,
        Checking,
        Savings,
        MoneyMarket,
        CreditLine,
    }
}