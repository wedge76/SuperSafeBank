using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.DomainEvents
{
    public static class CustomerEvents
    {
        public record CustomerCreated : BaseDomainEvent<Customer, Guid>
        {
            /// <summary>
            /// for deserialization
            /// </summary>
            private CustomerCreated() { }

            public CustomerCreated(Customer customer, string firstname, string lastname, Email email) : base(customer)
            {
                Firstname = firstname;
                Lastname = lastname;
                Email = email;
            }

            public string Firstname { get; init; }
            public string Lastname { get; init; }
            public Email Email { get; init; }

            public override void Apply(Customer customer)
            {
                customer.Create(this);
            }
        }

        public record AccountAdded : BaseDomainEvent<Customer, Guid>
        {
            private AccountAdded() { }

            public AccountAdded(Customer customer, Guid accountId) : base(customer)
            {
                AccountId = accountId;
            }

            public Guid AccountId { get; init; }

            public override void Apply(Customer customer)
            {
                customer.AddAccount(AccountId);
            }
        }
    }
}