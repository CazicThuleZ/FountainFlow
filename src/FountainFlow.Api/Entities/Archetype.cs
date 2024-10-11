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
    // Navigation properties
    public ICollection<ArchetypeBeat> ArchetypeBeats { get; set; } = new List<ArchetypeBeat>();
    public ICollection<ArchetypeGenre> ArchetypeGenres { get; set; } = new List<ArchetypeGenre>();
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
