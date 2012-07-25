using System;
using System.Collections.Generic;
using AccountManager.Code;
using AccountManager.Models.Commands;
using AccountManager.Models.Domain;
using AccountManager.Models.Events;

namespace AccountManager.Tests
{
    public class LockedAccountShouldNotAllowDebit : TestBase
    {
        readonly Guid _accountId = Guid.NewGuid();

        public override Dictionary<object, List<object>> GivenTheseEvents()
        {
            return new Dictionary<object, List<object>>
            {
                {_accountId, new List<object>
                    {
                        new AccountRegisteredEvent(_accountId, "John", "abc@example.com", 500),
                        new AccountLockedEvent(_accountId)
                    }
                }
            };
        }

        public override object WhenThisHappens()
        {
            return new DebitAccountCommand { AccountId = _accountId, Amount = 10 };
        }

        public override Exception ThisExceptionShouldOccur()
        {
            return new AccountLockedException(_accountId);
        }

        public override void RegisterHandler(MessageBus bus, IRepository repo)
        {
            var svc = new AccountApplicationService(repo);
            bus.RegisterHandler<DebitAccountCommand>(svc.Handle);
        }
    }
}