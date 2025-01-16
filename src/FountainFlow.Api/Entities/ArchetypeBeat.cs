using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

[Table("ArchetypeBeats")]
public class ArchetypeBeat
{
    public Guid Id { get; set; }
    public Guid ArchetypeId { get; set; }
    public int ParentSequence { get; set; }
    public int? ChildSequence { get; set; }
    public int? GrandchildSequence { get; set; }    
    [Column(TypeName = "text")]
    public string Prompt { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }    
    public int PercentOfStory { get; set; }
    public Archetype Archetype { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
