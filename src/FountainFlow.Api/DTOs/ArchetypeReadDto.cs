using System;
using System.Collections.Generic;

namespace FountainFlow.Api.DTOs;

public class ArchetypeReadDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; }
    public ICollection<Guid> ArchetypeBeatIds { get; set; }
    public ICollection<Guid> ArchetypeGenreIds { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
