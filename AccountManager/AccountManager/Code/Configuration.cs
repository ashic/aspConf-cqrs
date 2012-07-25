using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Models.Commands;
using AccountManager.Models.Domain;
using AccountManager.Models.Events;
using AccountManager.Models.ReadModel;
using AccountManager.Models.ReadModel.Admin;
using AccountManager.Models.ReadModel.Public;

namespace AccountManager.Code
{
    public class Configuration
    {
        private readonly MessageBus _bus;
        private readonly ReadModelFacade _readModel;

        public MessageBus Bus { get { return _bus; } }
        public ReadModelFacade ReadModel { get { return _readModel; } }

        private static readonly Configuration Config = new Configuration();
        public static Configuration Instance()
        {
            return Config;
        }

        private Configuration()
        {
            _bus = new MessageBus();
            var eventStore = new SqlEventStore(_bus);
            var repository = new DomainRepository(eventStore);

            var commandService = new AccountApplicationService(repository);
            _bus.RegisterHandler<RegisterAccountCommand>(commandService.Handle);
            _bus.RegisterHandler<DebitAccountCommand>(commandService.Handle);
            _bus.RegisterHandler<UnlockAccountCommand>(commandService.Handle);

            var infoProjection = new AccountInfoProjection();
            _bus.RegisterHandler<AccountRegisteredEvent>(infoProjection.Handle);
            _bus.RegisterHandler<AccountLockedEvent>(infoProjection.Handle);
            _bus.RegisterHandler<AccountUnlockedEvent>(infoProjection.Handle);

            var balanceProjection = new AccountBalanceProjection();
            _bus.RegisterHandler<AccountRegisteredEvent>(balanceProjection.Handle);
            _bus.RegisterHandler<AccountDebitedEvent>(balanceProjection.Handle);

            var notification = new NotificationProjection();
            _bus.RegisterHandler<AccountRegisteredEvent>(notification.Handle);
            _bus.RegisterHandler<AccountDebitedEvent>(notification.Handle);
            _bus.RegisterHandler<AccountLockedEvent>(notification.Handle);
            _bus.RegisterHandler<AccountUnlockedEvent>(notification.Handle);
            _bus.RegisterHandler<OverdrawAttemptedEvent>(notification.Handle);

            _readModel = new ReadModelFacade(balanceProjection, infoProjection, notification);

            var events = eventStore.GetAllEventsEver();
            _bus.Publish(events);
        }
    }
}