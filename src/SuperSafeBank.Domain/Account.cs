using System;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain;

public record Account : BaseAggregateRoot<Account, Guid>
{
    private Account()
    {
    }

    public Account(Guid id, Customer owner, Currency currency) : base(id)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        if (currency == null)
        {
            throw new ArgumentNullException(nameof(currency));
        }

        Append(new AccountEvents.AccountCreated(this, owner, currency));
    }

    public Guid OwnerId { get; private set; }

    public Money Balance { get; private set; }

    public void Withdraw(Money amount, ICurrencyConverter currencyConverter)
    {
        if (amount.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "amount cannot be negative");
        }

        var normalizedAmount = currencyConverter.Convert(amount, Balance.Currency);
        if (normalizedAmount.Value > Balance.Value)
        {
            throw new AccountTransactionException($"unable to withdrawn {normalizedAmount} from account {Id}", this);
        }

        Append(new AccountEvents.Withdrawal(this, amount));
    }

    public void Deposit(Money amount, ICurrencyConverter currencyConverter)
    {
        if (amount.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "amount cannot be negative");
        }

        var normalizedAmount = currencyConverter.Convert(amount, Balance.Currency);

        Append(new AccountEvents.Deposit(this, normalizedAmount));
    }

    protected override void When(IDomainEvent<Account, Guid> @event)
    {
        @event.Apply(this);
    }

    public static Account Create(Guid accountId, Customer owner, Currency currency)
    {
        var account = new Account(accountId, owner, currency);
        owner.AddAccount(account);
        return account;
    }

    internal void AddDeposit(Money amount)
    {
        Balance = Balance.Add(amount);
    }

    internal void Withdraw(Money amount)
    {
        Balance = Balance.Subtract(amount);
    }

    internal void Create(AccountEvents.AccountCreated accountCreated)
    {
        Id = accountCreated.AggregateId;
        Balance = Money.Zero(accountCreated.Currency);
        OwnerId = accountCreated.OwnerId;
    }
}