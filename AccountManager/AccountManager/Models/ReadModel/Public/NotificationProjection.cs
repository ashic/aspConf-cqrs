using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Models.Events;

namespace AccountManager.Models.ReadModel.Public
{
    public class NotificationProjection
    {
        public List<Notification> Notifications = new List<Notification>();
 
        public void Handle(AccountRegisteredEvent e)
        {
            Notifications.Add(new Notification(e.AccountId, string.Format("Account registered with initial balance of {0}.", e.InitialBalance)));
        }

        public void Handle(AccountDebitedEvent e)
        {
            Notifications.Add(new Notification(e.AccountId, string.Format("Debited {0}.", e.Amount)));
        }

        public void Handle(AccountLockedEvent e)
        {
            Notifications.Add(new Notification(e.AccountId, string.Format("Account locked.")));
        }

        public void Handle(AccountUnlockedEvent e)
        {
            Notifications.Add(new Notification(e.AccountId, string.Format("Account unlocked.")));
        }

        public void Handle(OverdrawAttemptedEvent e)
        {
            Notifications.Add(new Notification(e.AccountId, string.Format("Overdraw of {0} attempted.", e.Amount)));
        }
    }
}