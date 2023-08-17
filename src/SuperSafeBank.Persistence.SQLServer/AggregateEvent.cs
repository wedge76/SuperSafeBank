using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.SQLServer;

internal record AggregateEvent
{
    private AggregateEvent()
    {
    }

    required public string AggregateId { get; init; }

    required public long AggregateVersion { get; init; }

    required public string EventType { get; init; }

    required public byte[] Data { get; init; }

    required public DateTimeOffset Timestamp { get; init; }

    public static AggregateEvent Create<TA, TKey>(IDomainEvent<TA, TKey> @event, IEventSerializer eventSerializer) where TA : IAggregateRoot<TA, TKey>
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (eventSerializer is null)
        {
            throw new ArgumentNullException(nameof(eventSerializer));
        }

        var data = eventSerializer.Serialize(@event);
        var eventType = @event.GetType();

        return new AggregateEvent
               {
                   AggregateId = @event.AggregateId.ToString(),
                   AggregateVersion = @event.AggregateVersion,
                   EventType = eventType.AssemblyQualifiedName,
                   Data = data,
                   Timestamp = @event.When
               };
    }
}