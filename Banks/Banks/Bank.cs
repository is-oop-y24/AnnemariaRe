using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Banks.Accounts;
using Banks.Clients;
using Banks.Tools;
using Banks.Transactions;
using Transaction = Banks.Transactions.Transaction;

namespace Banks.Banks
{
    public class Bank
    {
        private readonly AccountFactory _accountFactory;

        public Bank(string name, decimal debitInterest, Dictionary<int, decimal> depositInterests, decimal creditComission, int creditLimit)
        {
            Id = Guid.NewGuid();
            if (string.IsNullOrEmpty(name)) throw new NullOrEmptyBanksException("Name cannot be null");
            Name = name;
            _accountFactory = new AccountFactory();
            _accountFactory.SetValues(creditLimit, creditComission, debitInterest, depositInterests);
            Accounts = new List<Account>();
            Clients = new Dictionary<Client, List<Account>>();
        }

        public Guid Id { get; }
        public string Name { get; }
        public List<Account> Accounts { get; }
        public Dictionary<Client, List<Account>> Clients { get; }

        public Client AddNewClient(Client client)
        {
            if (client == null) throw new NullOrEmptyBanksException("Client cannot be null");
            if (Clients.ContainsKey(client)) throw new BanksException("Bank already has current client");
            Clients.Add(client, new List<Account>());
            return client;
        }

        public void RemoveClient(Client client)
        {
            Clients?.Remove(client);
        }

        public Account CreateDebitAccount(Client client, decimal money)
        {
            if (!Clients.ContainsKey(client)) throw new BanksException("Cannot find current client");
            var account = _accountFactory.CreateAccount("Debit", client, money);
            Accounts.Add(account);
            Clients[client]?.Add(account);
            return account;
        }

        public Account CreateDepositAccount(Client client, decimal money)
        {
            if (!Clients.ContainsKey(client)) throw new BanksException("Cannot find current client");
            var account = _accountFactory.CreateAccount("Deposit", client, money, _accountFactory.ExpirationDate);
            Accounts.Add(account);
            Clients[client]?.Add(account);
            return account;
        }

        public Account CreateCreditAccount(Client client, decimal money)
        {
            if (!Clients.ContainsKey(client)) throw new BanksException("Cannot find current client");
            var account = _accountFactory.CreateAccount("Credit", client, money);
            Accounts.Add(account);
            Clients[client]?.Add(account);
            return account;
        }

        public void ReplenishMoney(Guid accountId, decimal money)
        {
            var account = Accounts.FirstOrDefault(acc => acc.Id == accountId);
            if (account is null) throw new NullOrEmptyBanksException("Cannot find an account");
            var transaction = new Transaction(account);
            transaction.Replenish(money);
        }

        public void WithdrawMoney(Guid accountId, decimal money)
        {
            var account = Accounts.FirstOrDefault(acc => acc.Id == accountId);
            if (account is null) throw new NullOrEmptyBanksException("Cannot find an account");
            var transaction = new Transaction(account);
            transaction.Withdraw(money);
        }

        public void TransferMoney(Guid withdrawAccountId, Guid replenishAccountId, decimal money)
        {
            var account1 = Accounts.FirstOrDefault(acc => acc.Id == withdrawAccountId);
            var account2 = Accounts.FirstOrDefault(acc => acc.Id == replenishAccountId);
            if (account1 is null || account2 is null) throw new NullOrEmptyBanksException("Cannot find an account");
            var transaction = new Transaction(account1);
            transaction.Transfer(account2, money);
        }

        public void UndoTransaction(Guid accountId)
        {
            var account = Accounts.FirstOrDefault(acc => acc.Id == accountId);
            if (account is null) throw new NullOrEmptyBanksException("Cannot find an account");
            var transaction = new Transaction(account);
            transaction.UndoTransaction();
        }

        public void UpdateDebitInterest(decimal interest)
        {
            _accountFactory.SetDebitInterest(interest);
        }

        public void UpdateDepositInterests(Dictionary<int, decimal> interests)
        {
            _accountFactory.SetDepositInterests(interests);
        }

        public void UpdateCreditLimit(int limit)
        {
            _accountFactory.SetCreditLimit(limit);
        }

        public void UpdateCreditComission(decimal comission)
        {
            _accountFactory.SetCreditComission(comission);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"BANK - {Name}");
            sb.AppendLine("List of clients: ");

            foreach (var client in Clients)
            {
                sb.AppendLine(client.Key.ToString());
                sb.AppendLine("Client accounts: ");
                foreach (var account in client.Value)
                {
                    sb.AppendLine(account.ToString());
                    sb.AppendLine("-------------------");
                }
            }

            sb.AppendLine("---------------------------------");
            return sb.ToString();
        }
    }
}