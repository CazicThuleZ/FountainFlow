using System;
using FountainFlowUI.DTOs;
using FountainFlowUI.Interfaces;

namespace FountainFlowUI.Repository;

public class ArchetypesRepository : IArchetypesRepository
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public ArchetypesRepository(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["ApiBaseUrl"];                
    }

    public async Task<List<ArchetypeDto>> GetArchetypesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ArchetypeDto>>("{_apiBaseUrl}/archetypes/api/v1/Archetypes");

        return response;
    }
}
