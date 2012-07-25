using System;

namespace AccountManager.Models.ReadModel.Admin
{
    public class AccountInfo
    {
        public Guid AccountId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public bool IsLocked { get; private set; }

        public AccountInfo(Guid accountId, string name, string email)
        {
            AccountId = accountId;
            Name = name;
            Email = email;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }
    }
}