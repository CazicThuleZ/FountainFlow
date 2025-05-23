using System;
using System.Text;
using System.Text.Json;
using System.Transactions;
using FountainFlowUI.DTOs;
using FountainFlowUI.Helpers;
using FountainFlowUI.Interfaces;
using FountainFlowUI.Models;

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

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No genres found for archetype {ArchetypeId}", archetypeId);
                return new List<ArchetypeGenreDto>();
            }

            // Now check for other error status codes
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content");
                return new List<ArchetypeGenreDto>();
            }

            var archetypeGenres = await response.Content.ReadFromJsonAsync<List<ArchetypeGenreDto>>();

            if (archetypeGenres == null || !archetypeGenres.Any())
            {
                _logger.LogWarning("No genres found in response");
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
            _logger.LogInformation("Fetching archetype beats from API for archetype {ArchetypeId}", archetypeId);

            using var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/v1.0/ArchetypeBeats/Archetype/{archetypeId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No beats found for archetype {ArchetypeId}", archetypeId);
                return new List<ArchetypeBeatDto>();
            }

            response.EnsureSuccessStatusCode();

            var archetypeBeats = await response.Content.ReadFromJsonAsync<List<ArchetypeBeatDto>>();

            if (!archetypeBeats.Any())
            {
                _logger.LogWarning("No beats found in response for archetype {ArchetypeId}", archetypeId);
                return new List<ArchetypeBeatDto>();
            }

            // Order beats hierarchically
            return archetypeBeats
                .OrderBy(b => b.ParentSequence)
                .ThenBy(b => b.ChildSequence)
                .ThenBy(b => b.GrandchildSequence)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching archetype beats for {ArchetypeId}", archetypeId);
            throw new RepositoryException($"Failed to fetch archetype beats for {archetypeId}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching archetype beats for {ArchetypeId}", archetypeId);
            throw new RepositoryException($"An unexpected error occurred while fetching archetype beats for {archetypeId}", ex);
        }
    }

    public async Task<bool> DeleteArchetypeAsync(Guid id)
    {
        var archetypeId = id.ToString();
        try
        {
            _logger.LogInformation("Attempting to delete archetype with ID {ArchetypeId}", archetypeId);

            using var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/v1.0/Archetypes/{archetypeId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Archetype with ID {ArchetypeId} not found", archetypeId);
                return false;
            }

            response.EnsureSuccessStatusCode();

            // API returns NoContent (204) on successful deletion
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while deleting archetype. Status code: {StatusCode}",
                ex.StatusCode);
            throw new RepositoryException($"Failed to delete archetype {archetypeId}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting archetype");
            throw new RepositoryException($"An unexpected error occurred while deleting archetype {archetypeId}", ex);
        }
    }

    public async Task<ArchetypeDto> CreateArchetypeAsync(ArchetypeDto archetypeDto)
    {
        try
        {
            _logger.LogInformation("Attempting to create new archetype with domain: {Domain}", archetypeDto.Domain);

            using var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/api/v1.0/Archetypes", archetypeDto);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content after creating archetype");
                return new ArchetypeDto();
            }

            var createdArchetype = await response.Content.ReadFromJsonAsync<ArchetypeDto>();

            if (createdArchetype == null)
            {
                _logger.LogWarning("Failed to deserialize created archetype response");
                return new ArchetypeDto();
            }

            return createdArchetype;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while creating archetype. Status code: {StatusCode}",
                ex.StatusCode);
            throw new RepositoryException("Failed to create archetype", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize created archetype response");
            throw new RepositoryException("Failed to parse created archetype data from the API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating archetype");
            throw new RepositoryException("An unexpected error occurred while creating the archetype", ex);
        }
    }
    
    public async Task<ArchetypeGenreDto> CreateArchetypeGenreAsync(ArchetypeGenreDto archetypeGenreDto)
    {
        try
        {
            archetypeGenreDto.Id = Guid.NewGuid();
            _logger.LogInformation("Attempting to create new archetype genre for archetype: {ArchetypeId}",
                archetypeGenreDto.ArchetypeId);

            using var response = await _httpClient.PostAsJsonAsync(
                $"{_apiBaseUrl}/api/v1.0/ArchetypeGenres",
                archetypeGenreDto);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("API returned empty content after creating archetype genre");
                return new ArchetypeGenreDto();
            }

            var createdGenre = await response.Content.ReadFromJsonAsync<ArchetypeGenreDto>();

            if (createdGenre == null)
            {
                _logger.LogWarning("Failed to deserialize created archetype genre response");
                return new ArchetypeGenreDto();
            }

            return createdGenre;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while creating archetype genre. Status code: {StatusCode}",
                ex.StatusCode);
            throw new RepositoryException("Failed to create archetype genre", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating archetype genre");
            throw new RepositoryException("An unexpected error occurred while creating the archetype genre", ex);
        }
    }

    public async Task<bool> DeleteArchetypeGenreAsync(Guid id)
    {
        var genreId = id.ToString();
        try
        {
            _logger.LogInformation("Attempting to delete archetype genre with ID {GenreId}", genreId);

            using var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/v1.0/ArchetypeGenres/{genreId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Archetype genre with ID {GenreId} not found", genreId);
                return false;
            }

            response.EnsureSuccessStatusCode();
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while deleting archetype genre. Status code: {StatusCode}",
                ex.StatusCode);
            throw new RepositoryException($"Failed to delete archetype genre {genreId}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting archetype genre");
            throw new RepositoryException($"An unexpected error occurred while deleting archetype genre {genreId}", ex);
        }
    }

    public async Task<bool> SaveBeatsAsync(SaveBeatsRequestDto request)
    {
        try
        {
            _logger.LogInformation("Saving beats for archetype {ArchetypeId}", request.ArchetypeId);

            ValidateSaveBeatsRequest(request);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"{_apiBaseUrl}/api/v1.0/ArchetypeBeats/SaveBeats",
                jsonContent
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to save beats. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving beats for archetype {ArchetypeId}",
                request.ArchetypeId);
            throw new RepositoryException(
                $"An unexpected error occurred while saving beats for archetype {request.ArchetypeId}",
                ex
            );
        }
    }
    
    public async Task<bool> ImportArchetypesAsync(List<ArchetypeExportModel> archetypes)
    {
        try
        {
            _logger.LogInformation("Importing {Count} archetypes", archetypes.Count);
            
            // Use a transaction scope to ensure all-or-nothing import
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
            foreach (var archetypeModel in archetypes)
            {
                // Create new archetype with new ID
                var archetypeDto = new ArchetypeDto
                {
                    Domain = archetypeModel.Archetype.Domain,
                    Description = archetypeModel.Archetype.Description,
                    Architect = archetypeModel.Archetype.Architect,
                    ExternalLink = archetypeModel.Archetype.ExternalLink,
                    Icon = archetypeModel.Archetype.Icon,
                    Rank = archetypeModel.Archetype.Rank,
                    ArchetypeBeatIds = new List<Guid>(),
                    ArchetypeGenreIds = new List<Guid>()
                };
                
                // Create the archetype
                var createdArchetype = await CreateArchetypeAsync(archetypeDto);
                
                if (createdArchetype == null || createdArchetype.Id == Guid.Empty)
                {
                    _logger.LogError("Failed to create archetype {Domain}", archetypeDto.Domain);
                    throw new RepositoryException($"Failed to create archetype {archetypeDto.Domain}");
                }
                
                // Create beats for this archetype
                foreach (var beat in archetypeModel.Beats)
                {
                    var beatDto = new ArchetypeBeatDto
                    {
                        ArchetypeId = createdArchetype.Id,
                        ParentSequence = beat.ParentSequence,
                        ChildSequence = beat.ChildSequence,
                        GrandchildSequence = beat.GrandchildSequence,
                        Name = beat.Name,
                        Description = beat.Description,
                        Prompt = beat.Prompt,
                        PercentOfStory = beat.PercentOfStory
                    };
                    
                    // Create the beat using the API
                    using var beatResponse = await _httpClient.PostAsJsonAsync(
                        $"{_apiBaseUrl}/api/v1.0/ArchetypeBeats", beatDto);
                    
                    if (!beatResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await beatResponse.Content.ReadAsStringAsync();
                        _logger.LogError("Failed to create beat {Name}. Status: {StatusCode}, Error: {Error}",
                            beatDto.Name, beatResponse.StatusCode, errorContent);
                        throw new RepositoryException($"Failed to create beat {beatDto.Name}");
                    }
                }
                
                // Create genres for this archetype
                foreach (var genre in archetypeModel.Genres)
                {
                    var genreDto = new ArchetypeGenreDto
                    {
                        ArchetypeId = createdArchetype.Id,
                        Name = genre.Name,
                        Description = genre.Description
                    };
                    
                    // Create the genre
                    var createdGenre = await CreateArchetypeGenreAsync(genreDto);
                    
                    if (createdGenre == null || createdGenre.Id == Guid.Empty)
                    {
                        _logger.LogError("Failed to create genre {Name}", genreDto.Name);
                        throw new RepositoryException($"Failed to create genre {genreDto.Name}");
                    }
                }
            }
            
            // Complete the transaction
            scope.Complete();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while importing archetypes");
            throw new RepositoryException("An unexpected error occurred while importing archetypes", ex);
        }
    }
    
    private void ValidateSaveBeatsRequest(SaveBeatsRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.ArchetypeId == Guid.Empty)
            throw new ArgumentException("ArchetypeId cannot be empty", nameof(request));

        if (request.Beats == null)
            throw new ArgumentException("Beats collection cannot be null", nameof(request));

        foreach (var beat in request.Beats)
        {
            ValidateBeat(beat);
        }

        ValidateSequenceHierarchy(request.Beats);
    }

    private void ValidateBeat(ArchetypeBeatDto beat)
    {
        if (beat == null)
            throw new ArgumentException("Beat cannot be null");


        if (string.IsNullOrWhiteSpace(beat.Name))
            throw new ArgumentException("Beat name cannot be empty");

        if (beat.ParentSequence < 0)
            throw new ArgumentException("ParentSequence cannot be negative");


        if (beat.ChildSequence.HasValue && beat.ChildSequence.Value < 0)
            throw new ArgumentException("ChildSequence cannot be negative");

        if (beat.GrandchildSequence.HasValue && beat.GrandchildSequence.Value < 0)
            throw new ArgumentException("GrandchildSequence cannot be negative");


        if (beat.PercentOfStory < 0 || beat.PercentOfStory > 100)
            throw new ArgumentException("PercentOfStory must be between 0 and 100");
    }
    
    private void ValidateSequenceHierarchy(List<ArchetypeBeatDto> beats)
    {
        // Only validate parent sequences for top-level beats (those without a ChildSequence)
        var parentBeats = beats.Where(b => !b.ChildSequence.HasValue).ToList();
        var parentSequences = parentBeats.Select(b => b.ParentSequence).ToList();

        if (parentSequences.Distinct().Count() != parentSequences.Count)
        {
            throw new ArgumentException("Duplicate parent sequences found among top-level beats");
        }

        // Validate child sequences within each parent
        foreach (var parentGroup in beats.Where(b => b.ChildSequence.HasValue)
                                       .GroupBy(b => b.ParentSequence))
        {
            var childSequences = parentGroup
                .Where(b => !b.GrandchildSequence.HasValue)
                .Select(b => b.ChildSequence.Value)
                .ToList();

            if (childSequences.Distinct().Count() != childSequences.Count)
            {
                throw new ArgumentException(
                    $"Duplicate child sequences found for parent {parentGroup.Key}"
                );
            }

            // Validate grandchild sequences within each child
            foreach (var childGroup in parentGroup.Where(b => b.GrandchildSequence.HasValue)
                                                .GroupBy(b => b.ChildSequence.Value))
            {
                var grandchildSequences = childGroup
                    .Select(b => b.GrandchildSequence.Value)
                    .ToList();

                if (grandchildSequences.Distinct().Count() != grandchildSequences.Count)
                {
                    throw new ArgumentException(
                        $"Duplicate grandchild sequences found for parent {parentGroup.Key} " +
                        $"and child {childGroup.Key}"
                    );
                }
            }
        }
    }
}