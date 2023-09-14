using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Entities.Entity;

[ExcludeFromCodeCoverage]
public class ScheduleEntity
{
    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(256)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Created { get; set; } = DateTime.Now!;

    public virtual List<DayEntity> Days { get; set; } = new();
}
