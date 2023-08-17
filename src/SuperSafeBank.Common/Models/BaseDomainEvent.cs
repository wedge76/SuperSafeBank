using System;

namespace SuperSafeBank.Common.Models
{
    public abstract record BaseDomainEvent<TA, TKey> : IDomainEvent<TA, TKey>
        where TA : IAggregateRoot<TA, TKey>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        protected BaseDomainEvent() { }

        protected BaseDomainEvent(TA aggregateRoot)
        {
            if(aggregateRoot is null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            this.AggregateVersion = aggregateRoot.Version;
            this.AggregateId = aggregateRoot.Id;
            this.When = DateTime.UtcNow;
        }

        public long AggregateVersion { get; private set; }
        public TKey AggregateId { get; private set; }
        public DateTime When { get; private set; }

        public abstract void Apply(TA aggregate);
    }
}