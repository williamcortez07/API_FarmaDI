namespace FarmaDiApi.Configurations;

public class MongoLogSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 30;
}