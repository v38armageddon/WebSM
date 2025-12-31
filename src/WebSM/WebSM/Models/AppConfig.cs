namespace WebSM.Models;

public record AppConfig
{
    public string? Environment { get; init; }
    public string DefaultTheme { get; init; } = "Default";
    public bool AlternateUserAgent { get; init; } = false;
}
