using System;
using System.Text.Json;
using FountainFlowUI.DTOs;
using FountainFlowUI.Helpers;
using FountainFlowUI.Interfaces;

namespace FountainFlowUI.Repository;

public class ArchetypesRepository : IArchetypesRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArchetypesRepository> _logger;
    private readonly string _apiBaseUrl;
    private readonly IConfiguration _configuration;

    public ArchetypesRepository(ILogger<ArchetypesRepository> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _apiBaseUrl = _configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl configuration is missing");
    }

    public async Task<List<ArchetypeDto>> GetArchetypesAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to fetch archetypes from API at {ApiUrl}", $"{_apiBaseUrl}/api/v1.0/Archetypes");

            using var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/v1.0/Archetypes");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content");
                return new List<ArchetypeDto>();
            }

            var archetypes = await response.Content.ReadFromJsonAsync<List<ArchetypeDto>>();

            if (archetypes == null)
            {
                _logger.LogWarning("Failed to deserialize archetypes response");
                return new List<ArchetypeDto>();
            }

            return archetypes;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching archetypes. Status code: {StatusCode}",
                ex.StatusCode);
            throw new RepositoryException("Failed to fetch archetypes from the API", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize archetypes response");
            throw new RepositoryException("Failed to parse archetypes data from the API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching archetypes");
            throw new RepositoryException("An unexpected error occurred while fetching archetypes", ex);
        }
    }

    public async Task<ArchetypeDto> GetArchetypeByIdAsync(Guid id)
    {
        var archetypeId = id.ToString();
        try
        {
            _logger.LogInformation("Attempting to fetch archetypes from API at {ApiUrl}", $"{_apiBaseUrl}/api/v1.0/Archetypes");

            using var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/v1.0/Archetypes/{archetypeId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content");
                return new ArchetypeDto();
            }

            var archetype = await response.Content.ReadFromJsonAsync<ArchetypeDto>();

            if (archetype == null)
            {
                _logger.LogWarning("Failed to deserialize archetypes response");
                return new ArchetypeDto();
            }

            return archetype;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching archetypes. Status code: {StatusCode}", ex.StatusCode);
            throw new RepositoryException($"Failed to fetch archetype for {archetypeId} ", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize archetypes response");
            throw new RepositoryException("Failed to parse archetype data from the API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching archetype");
            throw new RepositoryException($"An unexpected error occurred while fetching archetype {archetypeId}", ex);
        }
    }

    public async Task<List<ArchetypeGenreDto>> GetArchetypeGenresByArchetypeIdIdAsync(Guid id)
    {
        var archetypeId = id.ToString();
        try
        {
            _logger.LogInformation("Attempting to fetch archetype genres from API at {ApiUrl}", $"{_apiBaseUrl}/api/v1.0/ArchetypeGenres/Archetype/{archetypeId}");
            using var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/v1.0/ArchetypeGenres/Archetype/{archetypeId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No genres found for archetype {ArchetypeId}", archetypeId);
                return new List<ArchetypeGenreDto>();
            }

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content");
                return new List<ArchetypeGenreDto>();
            }

            var archetypeGenres = await response.Content.ReadFromJsonAsync<List<ArchetypeGenreDto>>();

            if (!archetypeGenres.Any())
            {
                _logger.LogWarning("Failed to deserialize archetype genres response");
                return new List<ArchetypeGenreDto>();
            }

            return archetypeGenres;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching archetype genres. Status code: {StatusCode}", ex.StatusCode);
            throw new RepositoryException($"Failed to fetch archetype genres for {archetypeId} ", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize archetype genre response");
            throw new RepositoryException("Failed to parse archetype genre data from the API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching archetype genres");
            throw new RepositoryException($"An unexpected error occurred while fetching archetype genres for {archetypeId}", ex);
        }
    }

    public async Task<List<ArchetypeBeatDto>> GetArchetypeBeatsByArchetypeIdIdAsync(Guid id)
    {
        var archetypeId = id.ToString();
        try
        {
            _logger.LogInformation("Attempting to fetch archetype beatss from API at {ApiUrl}", $"{_apiBaseUrl}/api/v1.0/ArchetypeBeats/Archetype/{archetypeId}");

            using var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/v1.0/ArchetypeBeats/Archetype/{archetypeId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No beats found for archetype {ArchetypeId}", archetypeId);
                return new List<ArchetypeBeatDto>();
            }

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content");
                return new List<ArchetypeBeatDto>();
            }

            var archetypeBeats = await response.Content.ReadFromJsonAsync<List<ArchetypeBeatDto>>();

            if (!archetypeBeats.Any())
            {
                _logger.LogWarning("Failed to deserialize archetype genres response");
                return new List<ArchetypeBeatDto>();
            }

            return archetypeBeats;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching archetype beats. Status code: {StatusCode}", ex.StatusCode);
            throw new RepositoryException($"Failed to fetch archetype beats for {archetypeId} ", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize archetype genre response");
            throw new RepositoryException("Failed to parse archetype beat data from the API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching archetype beats");
            throw new RepositoryException($"An unexpected error occurred while fetching archetype beats for {archetypeId}", ex);
        }
    }

    public async Task<bool> DeleteArchetypeAsync(Guid id)
    {
        return true;
        // var archetypeId = id.ToString();
        // try
        // {
        //     _logger.LogInformation("Attempting to delete archetype with ID {ArchetypeId}", archetypeId);

        //     using var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/v1.0/Archetypes/{archetypeId}");

        //     if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //     {
        //         _logger.LogWarning("Archetype with ID {ArchetypeId} not found", archetypeId);
        //         return false;
        //     }

        //     response.EnsureSuccessStatusCode();

        //     // API returns NoContent (204) on successful deletion
        //     return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        // }
        // catch (HttpRequestException ex)
        // {
        //     _logger.LogError(ex, "HTTP request failed while deleting archetype. Status code: {StatusCode}",
        //         ex.StatusCode);
        //     throw new RepositoryException($"Failed to delete archetype {archetypeId}", ex);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Unexpected error occurred while deleting archetype");
        //     throw new RepositoryException($"An unexpected error occurred while deleting archetype {archetypeId}", ex);
        // }
    }
}
