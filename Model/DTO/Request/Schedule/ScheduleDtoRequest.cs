using System.Diagnostics.CodeAnalysis;

namespace Entities.DTO.Request.Schedule;

[ExcludeFromCodeCoverage]
public record ScheduleDtoRequest
{
    public required string Name { get; init; }
    public required List<int> Days { get; init; }
}
