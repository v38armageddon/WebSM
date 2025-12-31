namespace WebSM.Lite.Models;

public record AppConfig
{
    public string? Environment { get; init; }
    public int Theme { get; init; }
}
