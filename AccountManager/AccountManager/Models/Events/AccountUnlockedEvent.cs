using System;

namespace AccountManager.Models.Events
{
    public class AccountUnlockedEvent
    {
        public Guid AccountId { get; private set; }

        public AccountUnlockedEvent(Guid accountId)
        {
            AccountId = accountId;
        }

        public override string ToString()
        {
            return string.Format("Account {0} unlocked.", AccountId);
        }
    }
}