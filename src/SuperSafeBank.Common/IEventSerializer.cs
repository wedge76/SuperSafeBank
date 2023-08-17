using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common
{
    public interface IEventSerializer
    {
        IDomainEvent<TA, TKey> Deserialize<TA, TKey>(string type, byte[] data) where TA : IAggregateRoot<TA, TKey>;
        IDomainEvent<TA, TKey> Deserialize<TA, TKey>(string type, string data) where TA : IAggregateRoot<TA, TKey>;
        byte[] Serialize<TA, TKey>(IDomainEvent<TA, TKey> @event) where TA : IAggregateRoot<TA, TKey>;
    }
}