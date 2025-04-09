using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Model
{
    class Account
    {
        public string BankID { get; }
        public string AccountID { get; }
        public AccountType AccountType { get; }
        public string Currency { get; }
        public List<Transaction> Transactions { get; }

        public Account(string bankID, string accountID, AccountType accountType, string currency)
        {
            BankID = bankID;
            AccountID = accountID;
            AccountType = accountType;
            Currency = currency;
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