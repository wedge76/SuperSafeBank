using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Service.Core.Persistence.EventStore
{
    public record CustomerEmail : BaseAggregateRoot<CustomerEmail, string>
    {
        private CustomerEmail() { }

        public CustomerEmail(string email, Guid customerId) : base(email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"'{nameof(email)}' cannot be null or whitespace.", nameof(email));
            }

            base.Append(new CustomerEmailEvents.CustomerEmailCreated(this, email, customerId));
        }

        public string Email { get; internal set; }
        public Guid CustomerId { get; internal set;  }

        protected override void When(IDomainEvent<CustomerEmail, string> @event)
        {
            @event.Apply(this);
        }
    }
}