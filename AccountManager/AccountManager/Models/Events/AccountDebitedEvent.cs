using System;

namespace AccountManager.Models.Events
{
    public class AccountDebitedEvent
    {
        public Guid AccountId { get; private set; }
        public int Amount { get; private set; }
        public int NewBalance { get; private set; }

        public AccountDebitedEvent(Guid accountId, int amount, int newBalance)
        {
            AccountId = accountId;
            Amount = amount;
            NewBalance = newBalance;
        }

        public override string ToString()
        {
            return string.Format("Account {0} debited {1}. New Balance is {2}.", AccountId, Amount, NewBalance);
        }
    }
}