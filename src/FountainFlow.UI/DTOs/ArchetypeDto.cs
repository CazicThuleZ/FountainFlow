using System;

namespace FountainFlowUI.DTOs;

    public class ArchetypeDto
    {
        public Guid Id { get; set; } 
        public string Domain { get; set; }
        public ICollection<Guid> ArchetypeBeatIds { get; set; }
        public ICollection<Guid> ArchetypeGenreIds { get; set; }
    }
