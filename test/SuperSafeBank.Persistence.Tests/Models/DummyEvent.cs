using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.Tests.Models
{
    public record DummyEvent : BaseDomainEvent<DummyAggregate, Guid>
    {
        private DummyEvent() { }
        public DummyEvent(DummyAggregate aggregate, string type) : base(aggregate)
        {
            Type = type;
        }

        public string Type { get; private set; }

        public override void Apply(DummyAggregate aggregate)
        {
            throw new NotImplementedException();
        }
    }
}