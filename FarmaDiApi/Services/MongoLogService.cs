using FarmaDiApi.Configurations;
using FarmaDiApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmaDiApi.Services;

public interface ILogService
{
    Task InsertLogAsync(SystemLog log);
}

public class MongoLogService : ILogService
{
    private readonly IMongoCollection<SystemLog> _collection;

    public MongoLogService(IOptions<MongoLogSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<SystemLog>(settings.CollectionName);

        InicializarIndices(settings.RetentionDays);
    }

    private void InicializarIndices(int retentionDays)
    {
        // 1. Índice TTL (Time-To-Live) para purga automática
        var ttlKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.Timestamp);
        var ttlOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromDays(retentionDays)
        };

        // 2. Índices de consulta frecuente para soporte técnico
        var traceKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.TraceId);
        var levelKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.Level);

        _collection.Indexes.CreateMany([
            new CreateIndexModel<SystemLog>(ttlKeys, ttlOptions),
            new CreateIndexModel<SystemLog>(traceKeys),
            new CreateIndexModel<SystemLog>(levelKeys)
        ]);
    }

    public async Task InsertLogAsync(SystemLog log)
    {
        try
        {
            await _collection.InsertOneAsync(log);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Fallo de persistencia en MongoDB: {ex.Message}");
        }
    }
}