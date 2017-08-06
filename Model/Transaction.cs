using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Accounts_Normaliser.Model
{
    class Transaction
    {
        public DateTimeOffset DatePosted { get; }
        public decimal Amount { get; }
        public string Name { get; }
        public string Memo { get; }

        public Transaction(DateTimeOffset datePosted, decimal amount, string name, string memo)
        {
            DatePosted = datePosted;
            Amount = amount;
            Name = name;
            Memo = memo;
        }
    }
}