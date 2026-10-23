using FarmaDiApi.Configurations;
using FarmaDiApi.Models;
using FarmaDiCore.Common;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FarmaDiApi.Services;

/// <summary>
/// Contrato de servicio para la gestión y consulta de auditoría y logs del sistema almacenados en MongoDB Atlas.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Inserta de forma asíncrona un nuevo registro de log de auditoría.
    /// </summary>
    Task InsertLogAsync(SystemLog log);

    /// <summary>
    /// Consulta registros de log paginados aplicando filtros opcionales de nivel de severidad y rango de fechas.
    /// </summary>
    Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsPagedAsync(
        int page,
        int limit,
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta registros de log paginados filtrados por una etiqueta o nivel específico (Information, Warning, Error).
    /// </summary>
    Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsByLevelPagedAsync(
        string level,
        int page,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta registros de log paginados comprendidos dentro de un rango de fechas.
    /// </summary>
    Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsByDateRangePagedAsync(
        DateTime startDate,
        DateTime endDate,
        int page,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta registros de log paginados correspondientes al día actual de la petición (UTC).
    /// </summary>
    Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsTodayPagedAsync(
        int page,
        int limit,
        CancellationToken cancellationToken = default);
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
                // 1. Índice TTL (Time-To-Live) para purga automática tras vencer el tiempo de retención
                var ttlKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.Timestamp);
                var ttlOptions = new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(retentionDays)
                };

                // 2. Índices de consulta frecuente para soporte técnico y ordenamiento cronológico eficiente
                var traceKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.TraceId);
                var levelKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.Level);
                var timestampDescKeys = Builders<SystemLog>.IndexKeys.Descending(x => x.Timestamp);
                var levelTimestampKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.Level).Descending(x => x.Timestamp);

                await _collection.Indexes.CreateManyAsync([
                    new CreateIndexModel<SystemLog>(ttlKeys, ttlOptions),
                    new CreateIndexModel<SystemLog>(traceKeys),
                    new CreateIndexModel<SystemLog>(levelKeys),
                    new CreateIndexModel<SystemLog>(timestampDescKeys),
                    new CreateIndexModel<SystemLog>(levelTimestampKeys)
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

    public async Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsPagedAsync(
        int page,
        int limit,
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (_collection == null)
        {
            return new ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>
            {
                Data = (Array.Empty<SystemLog>(), 0),
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = "No se pudo establecer conexión con el repositorio de logs en MongoDB Atlas."
            };
        }

        try
        {
            var builder = Builders<SystemLog>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrWhiteSpace(level))
            {
                // Búsqueda insensible a mayúsculas/minúsculas para coincidencia exacta de nivel
                var regex = new MongoDB.Bson.BsonRegularExpression($"^{Regex.Escape(level.Trim())}$", "i");
                filter &= builder.Regex(x => x.Level, regex);
            }

            if (startDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                filter &= builder.Gte(x => x.Timestamp, startUtc);
            }

            if (endDate.HasValue)
            {
                var endUtc = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                filter &= builder.Lte(x => x.Timestamp, endUtc);
            }

            // Conteo total para la paginación respetando los filtros aplicados
            var totalCountLong = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            int totalCount = totalCountLong > int.MaxValue ? int.MaxValue : (int)totalCountLong;

            if (totalCount == 0)
            {
                return new ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<SystemLog>(), 0),
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "No se encontraron registros de logs para los criterios especificados."
                };
            }

            // Consulta paginada ordenada por fecha descendente (más recientes primero)
            var logs = await _collection.Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return new ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>
            {
                Data = (logs, totalCount),
                IsSuccess = true,
                MessageCode = MessageCodes.Success,
                Message = "Logs obtenidos exitosamente."
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Fallo al consultar registros en MongoDB Atlas: {ex.Message}");
            return new ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>
            {
                Data = (Array.Empty<SystemLog>(), 0),
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = "Ocurrió un error inesperado al consultar los registros en la base de datos."
            };
        }
    }

    public async Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsByLevelPagedAsync(
        string level,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await GetLogsPagedAsync(page, limit, level: level, cancellationToken: cancellationToken);
    }

    public async Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsByDateRangePagedAsync(
        DateTime startDate,
        DateTime endDate,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        // Normalizamos el rango para abarcar desde el inicio exacto del primer día hasta el último tick del día final
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        return await GetLogsPagedAsync(page, limit, startDate: startUtc, endDate: endUtc, cancellationToken: cancellationToken);
    }

    public async Task<ServiceResponse<(IEnumerable<SystemLog> Items, int TotalCount)>> GetLogsTodayPagedAsync(
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        // El día de hoy en tiempo universal coordinado (UTC)
        var todayStartUtc = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var todayEndUtc = DateTime.SpecifyKind(todayStartUtc.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        return await GetLogsPagedAsync(page, limit, startDate: todayStartUtc, endDate: todayEndUtc, cancellationToken: cancellationToken);
    }
}
