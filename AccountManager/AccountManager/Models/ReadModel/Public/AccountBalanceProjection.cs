using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Models.Events;

namespace AccountManager.Models.ReadModel.Public
{
    public class AccountBalanceProjection
    {
        public List<AccountBalance> AccountBalances = new List<AccountBalance>();
 
        public void Handle(AccountRegisteredEvent @event)
        {
            if(AccountBalances.All(x=>x.AccountId != @event.AccountId))
                AccountBalances.Add(new AccountBalance(@event.AccountId, 
                    @event.Name, 
                    @event.InitialBalance));
        }

        public void Handle(AccountDebitedEvent @event)
        {
            var account = AccountBalances.FirstOrDefault(x => x.AccountId == @event.AccountId);

            if(account != null)
                account.Debit(@event.Amount);
        }
    }
}