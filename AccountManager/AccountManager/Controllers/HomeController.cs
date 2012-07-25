using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AccountManager.Code;
using AccountManager.Models.Commands;
using AccountManager.Models.Domain;
using AccountManager.Models.ReadModel;

namespace AccountManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var infos = Configuration.Instance().ReadModel.AccountInfos;
            return View(infos);
        }

        public ActionResult Admin()
        {
            var infos = Configuration.Instance().ReadModel.AccountInfos;
            return View(infos);
        }

        public ActionResult Public(Guid accountId)
        {
            var balance = Configuration.Instance().ReadModel.AccountBalances
                .First(x => x.AccountId == accountId);

            return View("Debit", balance);
        }

        [HttpPost]
        public ActionResult Debit(DebitAccountCommand command)
        {
            try
            {
                Configuration.Instance().Bus.Handle(command);
            }
            catch (AccountLockedException)
            {
                return RedirectToAction("Locked", new { accountId = command.AccountId });
            }

            return RedirectToAction("Debited", new { accountId = command.AccountId });
        }

        [HttpGet]
        public ActionResult Debited(Guid accountId)
        {
            return View(accountId);
        }

        [HttpGet]
        public ActionResult Locked(string message, Guid accountId)
        {
            return View(accountId);
        }

        [HttpPost]
        public ActionResult Unlock(Guid AccountId)
        {
            var command = new UnlockAccountCommand(AccountId);
            Configuration.Instance().Bus.Handle(command);

            return RedirectToAction("Unlocked");
        }

        [HttpGet]
        public ActionResult Unlocked()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            var command = new RegisterAccountCommand {AccountId = Guid.NewGuid()};
            return View(command);
        }

        [HttpGet]
        public ActionResult Notifications(Guid accountId)
        {
            var notifications = Configuration.Instance().ReadModel.Notifications.Where(x => x.AccountId == accountId).ToList();
            return PartialView(notifications);
        }

        [HttpPost]
        public ActionResult Register(RegisterAccountCommand command)
        {
            Configuration.Instance().Bus.Handle(command);
            return View("Registered");
        }
    }
}
