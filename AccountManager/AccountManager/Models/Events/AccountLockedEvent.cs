using System;

namespace AccountManager.Models.Events
{
    public class AccountLockedEvent
    {
        public Guid AccountId { get; private set; }

        public AccountLockedEvent(Guid accountId)
        {
            AccountId = accountId;
        }

        public override string ToString()
        {
            return string.Format("Account {0} is locked.", AccountId);
        }
    }
}