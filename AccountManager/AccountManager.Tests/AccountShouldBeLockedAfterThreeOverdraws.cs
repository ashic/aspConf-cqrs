using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountManager.Code;
using AccountManager.Models.Commands;
using AccountManager.Models.Domain;
using AccountManager.Models.Events;

namespace AccountManager.Tests
{
    public class AccountShouldBeLockedAfterThreeOverdraws : TestBase
    {
        readonly Guid _accountId = Guid.NewGuid();

        public override Dictionary<object, List<object>> GivenTheseEvents()
        {
            return new Dictionary<object, List<object>>
            {
                {_accountId, new List<object>
                    {
                        new AccountRegisteredEvent(_accountId, "John", "abc@example.com", 500),
                        new OverdrawAttemptedEvent(_accountId, 600),
                        new OverdrawAttemptedEvent(_accountId, 600)
                    }
                }
            };
        }

        public override object WhenThisHappens()
        {
            return new DebitAccountCommand { AccountId = _accountId, Amount = 700 };
        }

        public override IEnumerable<object> TheseEventsShouldOccur()
        {
            yield return new OverdrawAttemptedEvent(_accountId, 700);
            yield return new AccountLockedEvent(_accountId);
        }

        public override void RegisterHandler(MessageBus bus, IRepository repo)
        {
            var svc = new AccountApplicationService(repo);
            bus.RegisterHandler<DebitAccountCommand>(svc.Handle);
        }
    }
}
