using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FountainFlowUI.DTOs;

namespace FountainFlowUI.DTOs
{
    public class SaveBeatsRequestDto
    {
        public Guid ArchetypeId { get; set; }
        public List<ArchetypeBeatDto> Beats { get; set; } = new List<ArchetypeBeatDto>();
    }
}