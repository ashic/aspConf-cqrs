using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountManager.Models.Commands
{
    public class RegisterAccountCommand
    {
        [HiddenInput(DisplayValue = false)]
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int InitialBalance { get; set; }

        public override string ToString()
        {
            return string.Format("Request is made to register account {0} [{1} - {2}] with initial balance of {3}.", AccountId, Name, Email, InitialBalance);
        }
    }
}