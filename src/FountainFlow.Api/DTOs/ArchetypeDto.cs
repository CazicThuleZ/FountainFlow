using System;
using System.Collections.Generic;

namespace FountainFlow.Api.DTOs;

public class ArchetypeDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; }
    public string Description { get; set; }
    public string Architect { get; set; }
    public string ExternalLink { get; set; }
    public string Icon { get; set; }
    public ICollection<Guid> ArchetypeBeatIds { get; set; }
    public ICollection<Guid> ArchetypeGenreIds { get; set; }
}
