using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Code;
using AccountManager.Models.Events;

namespace AccountManager.Models.Domain
{
    public class Account : Aggregate
    {
        private int _balance;
        private int _numberOfOverdraws;
        private bool _isLocked;

        public Account(Guid id, string name, string email, int balance)
            : base(id)
        {
            Apply(new AccountRegisteredEvent(id, name, email, balance));            
        }

        private Account()
        {
        }

        public void Debit(int amount)
        {
            if (_isLocked)
                throw new AccountLockedException((Guid)Id);

            if(amount > _balance)
            {
                Apply(new OverdrawAttemptedEvent((Guid)Id, amount));

                if(_numberOfOverdraws == 3)
                    Apply(new AccountLockedEvent((Guid)Id));
            }
            else
                Apply(new AccountDebitedEvent((Guid)Id, amount, _balance - amount));
        }

        public void UnlockAccount()
        {
            if(_isLocked)
                Apply(new AccountUnlockedEvent((Guid)Id));
        }

        private void UpdateFrom(AccountRegisteredEvent @event)
        {
            Id = @event.AccountId;
            _balance = @event.InitialBalance;
        }

        private void UpdateFrom(OverdrawAttemptedEvent @event)
        {
            _numberOfOverdraws++;
        }

        private void UpdateFrom(AccountLockedEvent @event)
        {
            _isLocked = true;
        }

        private void UpdateFrom(AccountDebitedEvent @event)
        {
            _balance = @event.NewBalance;
        }

        private void UpdateFrom(AccountUnlockedEvent @event)
        {
            _numberOfOverdraws = 0;
            _isLocked = false;
        }
    }

    public class AccountLockedException : Exception
    {
        public AccountLockedException(Guid id) 
            : base(string.Format("Account {0} is locked and cannot be debited.", id))
        {
        }
    }
}