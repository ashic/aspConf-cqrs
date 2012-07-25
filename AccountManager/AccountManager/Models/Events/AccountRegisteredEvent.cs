using System;

namespace AccountManager.Models.Events
{
    public class AccountRegisteredEvent
    {
        public Guid AccountId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public int InitialBalance { get; private set; }

        public AccountRegisteredEvent(Guid accountId, string name, string email, int initialBalance)
        {
            AccountId = accountId;
            Name = name;
            Email = email;
            InitialBalance = initialBalance;
        }

        public override string ToString()
        {
            return string.Format("Account {0}[{1} - {2}] registered with intitial balance {3}.", AccountId, Name, Email, InitialBalance);
        }
    }
}