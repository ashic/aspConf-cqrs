using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountManager.Code;
using AccountManager.Models.Commands;

namespace AccountManager.Models.Domain
{
    public class AccountApplicationService
    {
        private readonly IRepository _repository;

        public AccountApplicationService(IRepository repository)
        {
            _repository = repository;
        }

        public void Handle(RegisterAccountCommand command)
        {
            var account = new Account(command.AccountId, command.Name, command.Email, command.InitialBalance);
            _repository.Save(account);
        }

        public void Handle(DebitAccountCommand command)
        {
            var account = _repository.GetById<Account>(command.AccountId);
            account.Debit(command.Amount);
            _repository.Save(account);
        }

        public void Handle(UnlockAccountCommand command)
        {
            var account = _repository.GetById<Account>(command.AccountId);
            account.UnlockAccount();
            _repository.Save(account);
        }
    }
}