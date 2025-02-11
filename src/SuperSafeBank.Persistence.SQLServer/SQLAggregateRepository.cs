﻿using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.SQLServer;

public class SQLAggregateRepository<TA, TKey> : IAggregateRepository<TA, TKey>
    where TA : class, IAggregateRoot<TA, TKey>
{
    private readonly string _dbConnString;
    private readonly IEventSerializer _eventSerializer;
    private readonly IAggregateTableCreator _tableCreator;

    public SQLAggregateRepository(SqlConnectionStringProvider connectionStringProvider,
                                  IAggregateTableCreator tableCreator,
                                  IEventSerializer eventSerializer)
    {
        if (connectionStringProvider is null)
        {
            throw new ArgumentNullException(nameof(connectionStringProvider));
        }

        _dbConnString = connectionStringProvider.ConnectionString;
        _tableCreator = tableCreator ?? throw new ArgumentNullException(nameof(tableCreator));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        if (aggregateRoot is null)
        {
            throw new ArgumentNullException(nameof(aggregateRoot));
        }

        if (!aggregateRoot.Events.Any())
        {
            return;
        }

        await _tableCreator.EnsureTableAsync<TA, TKey>(cancellationToken)
                           .ConfigureAwait(false);

        var entities = aggregateRoot.Events.Select(evt => AggregateEvent.Create(evt, _eventSerializer))
                                    .ToList();

        await using var dbConn = new SqlConnection(_dbConnString);
        await dbConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var transaction = dbConn.BeginTransaction();

        try
        {
            var lastVersion = await GetLastAggregateVersionAsync(aggregateRoot, dbConn, transaction)
                                  .ConfigureAwait(false);
            if (lastVersion >= aggregateRoot.Version)
            {
                throw new ArgumentOutOfRangeException(nameof(aggregateRoot), $"aggregate version mismatch, expected {aggregateRoot.Version}, got {lastVersion}");
            }

            var tableName = _tableCreator.GetTableName<TA, TKey>();
            var sql = $@"INSERT INTO {tableName} (aggregateId, aggregateVersion, eventType, data, timestamp)
                         VALUES (@aggregateId, @aggregateVersion, @eventType, @data, @timestamp);";

            await dbConn.ExecuteAsync(sql, entities, transaction)
                        .ConfigureAwait(false);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
    {
        await _tableCreator.EnsureTableAsync<TA, TKey>(cancellationToken)
                           .ConfigureAwait(false);

        var tableName = _tableCreator.GetTableName<TA, TKey>();
        var sql = $@"SELECT aggregateId, aggregateVersion, eventType, data, timestamp
                         FROM {tableName}
                         WHERE aggregateId = @aggregateId
                         ORDER BY aggregateVersion ASC";

        await using var dbConn = new SqlConnection(_dbConnString);
        await dbConn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var queryResult = await dbConn.QueryAsync<AggregateEvent>(sql, new { aggregateId = key })
                                          .ConfigureAwait(false);
        var aggregatesEvents = queryResult.ToList();
        if (aggregatesEvents.Any())
        {
            return null;
        }

        var events = new List<IDomainEvent<TA, TKey>>();

        foreach (var aggregateEvent in aggregatesEvents)
        {
            var @event = _eventSerializer.Deserialize<TA, TKey>(aggregateEvent.EventType, aggregateEvent.Data);
            events.Add(@event);
        }

        return BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion).ToList());
    }

    private async Task<long?> GetLastAggregateVersionAsync(TA aggregateRoot, SqlConnection dbConn, IDbTransaction transaction)
    {
        var tableName = _tableCreator.GetTableName<TA, TKey>();
        var sql = @$"SELECT TOP 1 aggregateVersion
                         FROM {tableName}
                         WHERE aggregateId = @aggregateId
                         ORDER BY aggregateVersion DESC";
        var result = await dbConn.QueryFirstOrDefaultAsync<long?>(sql, new { aggregateId = aggregateRoot.Id }, transaction)
                                 .ConfigureAwait(false);
        return result;
    }
}