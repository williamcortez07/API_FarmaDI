namespace FarmaDiApi.Models;

/// <summary>
/// DTO para la transferencia y exposición pública de los registros de auditoría y diagnóstico del sistema.
/// Desacopla la representación interna en MongoDB (Bson) del contrato público expuesto por la API.
/// </summary>
public class SystemLogDto
{
    public string? Id { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string? ClientIp { get; set; }
    public string? UserId { get; set; }
    public RequestDetailDto Request { get; set; } = new();
    public ResponseDetailDto Response { get; set; } = new();
    public ExceptionDetailDto? Exception { get; set; }
}

public class RequestDetailDto
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public string? Payload { get; set; }
}

public class ResponseDetailDto
{
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
}

public class ExceptionDetailDto
{
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string Source { get; set; } = string.Empty;
}

public static class SystemLogMappingExtensions
{
    /// <summary>
    /// Mapea la entidad de persistencia SystemLog a su DTO correspondiente de forma segura y eficiente.
    /// </summary>
    public static SystemLogDto ToDto(this SystemLog log)
    {
        return new SystemLogDto
        {
            Id = log.Id,
            TraceId = log.TraceId,
            Timestamp = log.Timestamp,
            Level = log.Level,
            Environment = log.Environment,
            ClientIp = log.ClientIp,
            UserId = log.UserId,
            Request = new RequestDetailDto
            {
                Method = log.Request.Method,
                Path = log.Request.Path,
                QueryString = log.Request.QueryString,
                Payload = log.Request.Payload
            },
            Response = new ResponseDetailDto
            {
                StatusCode = log.Response.StatusCode,
                ElapsedMilliseconds = log.Response.ElapsedMilliseconds
            },
            Exception = log.Exception == null ? null : new ExceptionDetailDto
            {
                Message = log.Exception.Message,
                StackTrace = log.Exception.StackTrace,
                Source = log.Exception.Source
            }
        };
    }
}
