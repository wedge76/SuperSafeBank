﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly IEnumerable<Assembly> _assemblies;
        private ConcurrentDictionary<string, Type> _typesCache = new();

        private static readonly Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public JsonEventSerializer(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies ?? new[] {Assembly.GetExecutingAssembly()};
        }

        public IDomainEvent<TA, TKey> Deserialize<TA, TKey>(string type, byte[] data) where TA : IAggregateRoot<TA, TKey>
        {
            var jsonData = System.Text.Encoding.UTF8.GetString(data);
            return this.Deserialize<TA, TKey>(type, jsonData);
        }

        public IDomainEvent<TA, TKey> Deserialize<TA, TKey>(string type, string data) where TA : IAggregateRoot<TA, TKey>
        {
            var eventType = _typesCache.GetOrAdd(type, _ => _assemblies.Select(a => a.GetType(type, false))
                                                                       .FirstOrDefault(t => t != null) ?? Type.GetType(type));
            if (null == eventType)
                throw new ArgumentOutOfRangeException(nameof(type), $"invalid event type: {type}");

            // as of 01/10/2020, "Deserialization to reference types without a parameterless constructor isn't supported."
            // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
            // apparently it's being worked on: https://github.com/dotnet/runtime/issues/29895

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject(data, eventType, JsonSerializerSettings);

            return (IDomainEvent<TA, TKey>)result;
        }

        public byte[] Serialize<TA, TKey>(IDomainEvent<TA, TKey> @event) where TA : IAggregateRoot<TA, TKey>
        {
            var json = System.Text.Json.JsonSerializer.Serialize((dynamic)@event);
            var data = Encoding.UTF8.GetBytes(json);
            return data;
        }
    }
}