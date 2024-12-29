using FountainFlowUI.Models;

public class EditBeatsViewModel
{
    public Guid ArchetypeId { get; set; }
    public string Domain { get; set; }
    public List<BeatViewModel> Beats { get; set; } = new List<BeatViewModel>();
}