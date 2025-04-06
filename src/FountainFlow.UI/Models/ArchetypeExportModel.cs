using System;
using System.Collections.Generic;

namespace FountainFlowUI.Models;

public class ArchetypeExportModel
{
    public ArchetypeViewModel Archetype { get; set; }
    public List<BeatViewModel> Beats { get; set; }
    public List<GenreViewModel> Genres { get; set; }
}