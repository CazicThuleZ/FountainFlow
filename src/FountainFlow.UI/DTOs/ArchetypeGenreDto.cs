using System;

namespace FountainFlowUI.DTOs;

public class ArchetypeGenreDto
{
    public Guid Id { get; set; }
    public Guid ArchetypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }    

}
