using System;

namespace FountainFlow.Api.DTOs;

public class ArchetypeBeatDto
{
    public Guid Id { get; set; }
    public Guid ArchetypeId { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int PercentOfStory { get; set; }
}
