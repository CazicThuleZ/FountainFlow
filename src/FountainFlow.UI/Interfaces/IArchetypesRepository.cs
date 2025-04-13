using System;
using System.Collections.Generic;
using FountainFlowUI.DTOs;
using FountainFlowUI.Models;

namespace FountainFlowUI.Interfaces;
public interface IArchetypesRepository
{
    Task<List<ArchetypeDto>> GetArchetypesAsync();
    Task<ArchetypeDto> GetArchetypeByIdAsync(Guid id);
    Task<List<ArchetypeGenreDto>> GetArchetypeGenresByArchetypeIdIdAsync(Guid id);
    Task<List<ArchetypeBeatDto>> GetArchetypeBeatsByArchetypeIdIdAsync(Guid id);
    Task<bool> DeleteArchetypeAsync(Guid id);
    Task<ArchetypeDto> CreateArchetypeAsync(ArchetypeDto archetypeDto);
    Task<ArchetypeGenreDto> CreateArchetypeGenreAsync(ArchetypeGenreDto archetypeGenreDto);
    Task<bool> DeleteArchetypeGenreAsync(Guid id);
    Task<bool> SaveBeatsAsync(SaveBeatsRequestDto request);
    Task<bool> ImportArchetypesAsync(List<ArchetypeExportModel> archetypes);
}
