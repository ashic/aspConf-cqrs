using System;

namespace AccountManager.Models.ReadModel.Public
{
    public class AccountBalance
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public int Balance { get; set; }

        public AccountBalance(Guid accountId, string name, int balance)
        {
            AccountId = accountId;
            Name = name;
            Balance = balance;
        }

        public void Debit(int amount)
        {
            Balance -= amount;
        }
    }
}