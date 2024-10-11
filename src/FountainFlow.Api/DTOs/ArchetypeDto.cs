using System;
using System.Collections.Generic;

namespace FountainFlow.Api.DTOs;

    public class ArchetypeDto
    {
        public Guid Id { get; set; } 
        public string Domain { get; set; }
        public ICollection<Guid> ArchetypeBeatIds { get; set; }
        public ICollection<Guid> ArchetypeGenreIds { get; set; }
    }
