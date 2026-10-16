using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmaDiApi.Models;

public class SystemLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("traceId")]
    public string TraceId { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("level")]
    public string Level { get; set; } = "Information"; // Information, Warning, Error

    [BsonElement("environment")]
    public string Environment { get; set; } = "Development";

    [BsonElement("clientIp")]
    public string? ClientIp { get; set; }

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("request")]
    public RequestDetail Request { get; set; } = new();

    [BsonElement("response")]
    public ResponseDetail Response { get; set; } = new();

    [BsonElement("exception")]
    [BsonIgnoreIfNull]
    public ExceptionDetail? Exception { get; set; }
}

public class RequestDetail
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public string? Payload { get; set; }
}

public class ResponseDetail
{
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
}

public class ExceptionDetail
{
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string Source { get; set; } = string.Empty;
}