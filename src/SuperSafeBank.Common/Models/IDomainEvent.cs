using System;

namespace SuperSafeBank.Common.Models
{
    public interface IDomainEvent<TA, out TKey> where TA : IAggregateRoot<TA, TKey>
    {
        long AggregateVersion { get; }
        TKey AggregateId { get; }
        DateTime When { get; }

        void Apply(TA aggregate);
    }
}