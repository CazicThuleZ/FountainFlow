using System;

namespace FountainFlow.Api.DTOs;

public class ArchetypeGenreDto
{
    public Guid Id { get; set; }
    public Guid ArchetypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
