using System;

namespace FountainFlow.Api.DTOs;

public class ArchetypeGenreReadDto
{
    public Guid Id { get; set; }
    public Guid ArchetypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}