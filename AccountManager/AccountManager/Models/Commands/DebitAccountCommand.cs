using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountManager.Models.Commands
{
    public class DebitAccountCommand
    {
        public Guid AccountId { get; set; }
        public int Amount { get; set; }

        public override string ToString()
        {
            return string.Format("An attempt is made to debit {0} from account {1}.", Amount, AccountId);
        }
    }
}