using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Entities.Entity;

[ExcludeFromCodeCoverage]
public class DayEntity : BaseEntity
{
    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime Day { get; set; } = DateTime.Now!;

    [ForeignKey("ScheduleId")]
    public virtual ScheduleEntity Schedule { get; set; } = null!;

    public virtual List<PersonEntity> People { get; set; } = new();
}
