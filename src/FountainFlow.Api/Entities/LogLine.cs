#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

[Table("LogLines")]
public class LogLine
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid? ArchetypeId { get; set; }
    public Archetype? Archetype { get; set; }
    public Guid? ThemeId { get; set; }
    public Theme? Theme { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }    
}