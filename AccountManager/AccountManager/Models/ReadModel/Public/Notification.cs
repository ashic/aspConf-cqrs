using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountManager.Models.ReadModel.Public
{
    public class Notification
    {
        public Guid AccountId { get; private set; }
        public string Message { get; private set; }

        public Notification(Guid accountId, string message)
        {
            AccountId = accountId;
            Message = message;
        }
    }
}