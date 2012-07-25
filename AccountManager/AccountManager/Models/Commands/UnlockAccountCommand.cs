using System;

namespace AccountManager.Models.Commands
{
    public class UnlockAccountCommand
    {
        public Guid AccountId { get; private set; }

        public UnlockAccountCommand(Guid accountId)
        {
            AccountId = accountId;
        }

        public override string ToString()
        {
            return string.Format("A request is made to unlock account {0}.", AccountId);
        }
    }
}