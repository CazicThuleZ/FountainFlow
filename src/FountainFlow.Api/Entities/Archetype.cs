using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

// An archetype is a universally recognized model or pattern in storytelling, representing common themes, characters, or
// plot structures that recur across different cultures, time periods, and genres.

[Table("Archetypes")]
public class Archetype
{
    public Guid Id { get; set; }
    public string Domain { get; set; }
    public string Description { get; set; }
    public string Architect { get; set; }
    public string ExternalLink { get; set; }
    public int Rank { get; set; } // Sort and priority order 
    public string Icon { get; set; }  // URL to an icon representing the archetype
    // Navigation properties
    public ICollection<ArchetypeBeat> ArchetypeBeats { get; set; } = new List<ArchetypeBeat>();
    public ICollection<ArchetypeGenre> ArchetypeGenres { get; set; } = new List<ArchetypeGenre>();
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
