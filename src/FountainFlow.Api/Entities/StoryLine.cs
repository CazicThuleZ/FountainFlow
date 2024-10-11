#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

[Table("StoryLines")]
public class StoryLine
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string LineType { get; set; } = string.Empty;
    public string LineText { get; set; } = string.Empty;
    public Guid StoryId { get; set; }
    public Story Story { get; set; } =  null!;
    public Guid? ArchetypeBeatId { get; set; }
    public ArchetypeBeat? ArchetypeBeat { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
