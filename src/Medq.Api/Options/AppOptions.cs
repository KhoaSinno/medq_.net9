namespace Medq.Api.Options;

public sealed class AppOptions
{
    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
    public int CacheTtlSeconds { get; set; } = 120;
}
