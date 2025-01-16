using System;

namespace FountainFlow.Api.DTOs;

public class ArchetypeBeatReadDto
{
    public Guid Id { get; set; }
    public Guid ArchetypeId { get; set; }
    public int ParentSequence { get; set; }
    public int? ChildSequence { get; set; }
    public int? GrandchildSequence { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Prompt { get; set; }
    public int PercentOfStory { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
