using System;

namespace FountainFlowUI.DTOs;

public class ArchetypeDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; }
    public string Description { get; set; }
    public string Architect { get; set; }
    public string ExternalLink { get; set; }
    public string Icon { get; set; }
    public int Rank { get; set; }
    public ICollection<Guid> ArchetypeBeatIds { get; set; }
    public ICollection<Guid> ArchetypeGenreIds { get; set; }
    public DateTime CreatedUTC { get; set; } // Added for creation date
}
