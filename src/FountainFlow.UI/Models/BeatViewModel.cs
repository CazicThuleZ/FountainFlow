using System;

namespace FountainFlowUI.Models;

public class BeatViewModel
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int PercentOfStory { get; set; }
    public Guid ArchetypeId { get; set; }
}
