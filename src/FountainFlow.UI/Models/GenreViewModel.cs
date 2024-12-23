using System;

namespace FountainFlowUI.Models;

public class GenreViewModel
{
    public Guid Id { get; set; }    
    public string Name { get; set; }
    public string Description { get; set; }    
    public Guid ArchetypeId { get; set; }

}
