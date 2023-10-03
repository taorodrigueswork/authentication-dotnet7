using System.Diagnostics.CodeAnalysis;

namespace API.Configurations;

[ExcludeFromCodeCoverage]
public record Seq
{
    public string? Url { get; init; }
}