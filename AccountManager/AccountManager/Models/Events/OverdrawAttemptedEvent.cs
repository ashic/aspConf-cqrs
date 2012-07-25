using System;

namespace AccountManager.Models.Events
{
    public class OverdrawAttemptedEvent
    {
        public Guid AccountId { get; private set; }
        public int Amount { get; private set; }

        public OverdrawAttemptedEvent(Guid accountId, int amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

        public override string ToString()
        {
            return string.Format("Overdraw of {0} attempted on account {1}.", Amount, AccountId);
        }
    }
}