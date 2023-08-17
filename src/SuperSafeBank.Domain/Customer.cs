using System;
using System.Collections.Generic;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;

namespace SuperSafeBank.Domain;

public record Customer : BaseAggregateRoot<Customer, Guid>
{
    private readonly HashSet<Guid> _accounts = new();

    private Customer()
    {
    }

    public Customer(Guid id,
                    string firstname,
                    string lastname,
                    Email email) : base(id)
    {
        if (String.IsNullOrWhiteSpace(firstname))
        {
            throw new ArgumentNullException(nameof(firstname));
        }

        if (String.IsNullOrWhiteSpace(lastname))
        {
            throw new ArgumentNullException(nameof(lastname));
        }

        if (email is null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        Append(new CustomerEvents.CustomerCreated(this,
                                                  firstname,
                                                  lastname,
                                                  email));
    }

    public string Firstname { get; private set; }

    public string Lastname { get; private set; }

    public Email Email { get; private set; }

    public IReadOnlyCollection<Guid> Accounts => _accounts;

    public void AddAccount(Account account)
    {
        if (account is null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        if (_accounts.Contains(account.Id))
        {
            return;
        }

        Append(new CustomerEvents.AccountAdded(this, account.Id));
    }

    protected override void When(IDomainEvent<Customer, Guid> @event)
    {
        @event.Apply(this);
    }

    public static Customer Create(Guid customerId,
                                  string firstName,
                                  string lastName,
                                  string email)
    {
        return new Customer(customerId,
                            firstName,
                            lastName,
                            new Email(email));
    }

    internal void AddAccount(Guid accountId)
    {
        _accounts.Add(accountId);
    }

    internal void Create(CustomerEvents.CustomerCreated customerCreated)
    {
        Id = customerCreated.AggregateId;
        Firstname = customerCreated.Firstname;
        Lastname = customerCreated.Lastname;
        Email = customerCreated.Email;
    }
}