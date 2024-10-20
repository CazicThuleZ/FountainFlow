using System;
using FountainFlowUI.DTOs;

namespace FountainFlowUI.Interfaces;
public interface IArchetypesRepository
{
    Task<List<ArchetypeDto>> GetArchetypesAsync();
}
