using FarmaDiApi.Configurations;
using FarmaDiApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace FarmaDiApi.Services;

public interface ILogService
{
    Task InsertLogAsync(SystemLog log);
}

public class MongoLogService : ILogService
{
    private readonly IMongoCollection<SystemLog>? _collection;

    public MongoLogService(IOptions<MongoLogSettings> options)
    {
        try
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<SystemLog>(settings.CollectionName);

            InicializarIndices(settings.RetentionDays);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] No se pudo inicializar la conexión con MongoDB: {ex.Message}");
        }
    }

    private void InicializarIndices(int retentionDays)
    {
        if (_collection == null) return;

        Task.Run(async () =>
        {
            try
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

                await _collection.Indexes.CreateManyAsync([
                    new CreateIndexModel<SystemLog>(ttlKeys, ttlOptions),
                    new CreateIndexModel<SystemLog>(traceKeys),
                    new CreateIndexModel<SystemLog>(levelKeys)
                ]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] No se pudieron inicializar los índices de MongoDB: {ex.Message}");
            }
        });
    }

    public async Task InsertLogAsync(SystemLog log)
    {
        if (_collection == null) return;

        try
        {
            await _collection.InsertOneAsync(log);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Fallo de persistencia de auditoría en MongoDB: {ex.Message}");
        }
    }
}
