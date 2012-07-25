using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Models.Events;

namespace AccountManager.Models.ReadModel.Admin
{
    public class AccountInfoProjection
    {
        public List<AccountInfo> AccountInfos = new List<AccountInfo>();
 
        public void Handle(AccountRegisteredEvent @event)
        {
            if(AccountInfos.All(x => x.AccountId != @event.AccountId))
                AccountInfos.Add(new AccountInfo(@event.AccountId, 
                    @event.Name, 
                    @event.Email));
        }

        public void Handle(AccountLockedEvent @event)
        {
            var account = AccountInfos.FirstOrDefault(x => x.AccountId == @event.AccountId);

            if (account != null)
                account.Lock();
        }

        public void Handle(AccountUnlockedEvent @event)
        {
            var account = AccountInfos.FirstOrDefault(x => x.AccountId == @event.AccountId);

            if (account != null)
                account.Unlock();
        }
    }
}