﻿using System;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain
{
    public record Account : BaseAggregateRoot<Account, Guid>
    {
        private Account() { }

        public Account(Guid id, Customer owner, Currency currency) : base(id)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));

            this.Append(new AccountEvents.AccountCreated(this, owner, currency));
        }

        public Guid OwnerId { get; private set; }
        public Money Balance { get; private set; }

        public void Withdraw(Money amount, ICurrencyConverter currencyConverter)
        {
            if (amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount),"amount cannot be negative");

            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            if (normalizedAmount.Value > this.Balance.Value)
                throw new AccountTransactionException($"unable to withdrawn {normalizedAmount} from account {this.Id}", this);

            this.Append(new AccountEvents.Withdrawal(this, amount));
        }

        public void Deposit(Money amount, ICurrencyConverter currencyConverter)
        {
            if(amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "amount cannot be negative");

            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);

            this.Append(new AccountEvents.Deposit(this, normalizedAmount));
        }

        protected override void When(IDomainEvent<Account, Guid> @event)
        {
            @event.Apply(this);

            switch (@event)
            {
                case AccountEvents.AccountCreated c:
                    this.Id = c.AggregateId;
                    this.Balance = Money.Zero(c.Currency);
                    this.OwnerId = c.OwnerId;
                    break;
                case AccountEvents.Withdrawal w:
                    this.Balance = this.Balance.Subtract(w.Amount);
                    break;
                case AccountEvents.Deposit d:
                    this.Balance = this.Balance.Add(d.Amount);
                    break;
            }
        }

        public static Account Create(Guid accountId, Customer owner, Currency currency)
        {
            var account = new Account(accountId, owner, currency);
            owner.AddAccount(account);
            return account;
        }
    }
}