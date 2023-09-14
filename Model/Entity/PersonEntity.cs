﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Entities.Entity;

[ExcludeFromCodeCoverage]
public class PersonEntity
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(256)")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "varchar(128)")]
    public string Phone { get; set; } = string.Empty;

    [Column(TypeName = "varchar(128)")]
    public string Email { get; set; } = string.Empty;

    public virtual List<DayEntity> Days { get; set; } = new();
}
