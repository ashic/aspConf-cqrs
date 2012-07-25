using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Models.ReadModel.Admin;
using AccountManager.Models.ReadModel.Public;

namespace AccountManager.Models.ReadModel
{
    public class ReadModelFacade
    {
        public List<AccountBalance> AccountBalances { get; private set; }
        public List<AccountInfo> AccountInfos { get; private set; }
        public List<Notification> Notifications { get; private set; }

        public ReadModelFacade(AccountBalanceProjection balance, AccountInfoProjection info, NotificationProjection notification)
        {
            AccountBalances = balance.AccountBalances;
            AccountInfos = info.AccountInfos;
            Notifications = notification.Notifications;
        }
    }
}