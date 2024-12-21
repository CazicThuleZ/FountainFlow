using System;
using FountainFlowUI.DTOs;

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
}
