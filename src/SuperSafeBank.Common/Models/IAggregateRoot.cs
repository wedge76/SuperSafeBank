using System.Collections.Generic;

namespace SuperSafeBank.Common.Models
{
    public interface IAggregateRoot<TA, out TKey> : IEntity<TKey> where TA : IAggregateRoot<TA, TKey>
    {
        long Version { get; }
        IReadOnlyCollection<IDomainEvent<TA, TKey>> Events { get; }
        void ClearEvents();
    }
}