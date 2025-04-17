using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FountainFlowUI.DTOs;
using FountainFlowUI.Interfaces;
using FountainFlowUI.Models;
using Microsoft.AspNetCore.Hosting; // Added for IWebHostEnvironment
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FountainFlowUI.Controllers
{
    public class ArchetypesController : Controller
    {
        private readonly ILogger<ArchetypesController> _logger;
        private readonly IArchetypesRepository _archetypesRepository;
        private readonly IWebHostEnvironment _environment; // Added

        public ArchetypesController(ILogger<ArchetypesController> logger, IArchetypesRepository archetypesRepository, IWebHostEnvironment environment) // Added environment
        {
            _logger = logger;
            _archetypesRepository = archetypesRepository;
            _environment = environment; // Added
        }

        public IActionResult Archetype() => View();

        [HttpGet]
        public async Task<IActionResult> GetArchetypes()
        {
            try
            {
                var archetypeDtos = await _archetypesRepository.GetArchetypesAsync();
                var archetypeViewModels = archetypeDtos.Select(MapToViewModel)
                    .OrderBy(a => a.Rank) // Sort by Rank
                    .ToList();

                return Json(archetypeViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving archetypes");
                return StatusCode(500, "An error occurred while retrieving archetypes");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArchetype(Guid id)
        {
            try
            {
                var archetypeDto = await _archetypesRepository.GetArchetypeByIdAsync(id);
                if (archetypeDto == null || archetypeDto.Id == Guid.Empty)
                {
                    return NotFound($"Archetype with ID {id} not found");
                }

                var archetypeViewModel = MapToViewModel(archetypeDto);

                // Get beats for this archetype
                var beats = await _archetypesRepository.GetArchetypeBeatsByArchetypeIdIdAsync(id);
                archetypeViewModel.Beats = beats.Select(MapToBeatViewModel).ToList();

                // Get genres for this archetype
                var genres = await _archetypesRepository.GetArchetypeGenresByArchetypeIdIdAsync(id);
                archetypeViewModel.Genres = genres.Select(MapToGenreViewModel).ToList();

                return Json(archetypeViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving archetype with ID {ArchetypeId}", id);
                return StatusCode(500, $"An error occurred while retrieving archetype with ID {id}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArchetypeBeats(Guid archetypeId)
        {
            try
            {
                var beatDtos = await _archetypesRepository.GetArchetypeBeatsByArchetypeIdIdAsync(archetypeId);
                var beatViewModels = beatDtos.Select(MapToBeatViewModel).ToList();

                return Json(beatViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving beats for archetype with ID {ArchetypeId}", archetypeId);
                return StatusCode(500, $"An error occurred while retrieving beats for archetype with ID {archetypeId}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArchetypeGenres(Guid archetypeId)
        {
            try
            {
                var genreDtos = await _archetypesRepository.GetArchetypeGenresByArchetypeIdIdAsync(archetypeId);
                var genreViewModels = genreDtos.Select(MapToGenreViewModel).ToList();

                return Json(genreViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving genres for archetype with ID {ArchetypeId}", archetypeId);
                return StatusCode(500, $"An error occurred while retrieving genres for archetype with ID {archetypeId}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateArchetype([FromForm] ArchetypeViewModel viewModel, IFormFile iconFile) // Changed to FromForm, added iconFile
        {
            if (!ModelState.IsValid)
            {
                // Consider returning specific validation errors
                return BadRequest(ModelState);
            }

            string relativeImagePath = null; // Initialize path as null

            try
            {
                // --- File Upload Handling ---
                if (iconFile != null && iconFile.Length > 0)
                {
                    // Basic validation (consider adding more robust checks - size, type)
                    if (iconFile.Length > 5 * 1024 * 1024) // Example: 5MB limit
                    {
                        return BadRequest("Icon file size exceeds the limit (5MB).");
                    }
                    if (!iconFile.ContentType.StartsWith("image/"))
                    {
                         return BadRequest("Invalid file type. Please upload an image.");
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "Thumbnails");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    // Generate a unique filename to prevent conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(iconFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    _logger.LogInformation("Saving uploaded icon file to: {FilePath}", filePath);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await iconFile.CopyToAsync(fileStream);
                    }

                    // Use forward slashes for the relative web path
                    relativeImagePath = $"/images/Thumbnails/{uniqueFileName}";
                    _logger.LogInformation("Relative image path set to: {RelativePath}", relativeImagePath);
                }
                else
                {
                    _logger.LogInformation("No icon file provided for archetype creation.");
                }
                // --- End File Upload Handling ---

                var archetypeDto = MapToDto(viewModel);
                archetypeDto.Icon = relativeImagePath; // Set the path (will be null if no file uploaded)

                var createdArchetype = await _archetypesRepository.CreateArchetypeAsync(archetypeDto);

                // Map the *created* DTO back to ViewModel for the response
                var createdViewModel = MapToViewModel(createdArchetype);

                return CreatedAtAction(nameof(GetArchetype), new { id = createdViewModel.Id }, createdViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating archetype with domain {Domain}", viewModel.Domain);
                // Avoid exposing detailed exception info to the client in production
                return StatusCode(500, "An internal error occurred while creating the archetype.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateArchetypeGenre([FromBody] GenreViewModel viewModel)
        {
            try
            {
                var genreDto = MapToDto(viewModel);
                var createdGenre = await _archetypesRepository.CreateArchetypeGenreAsync(genreDto);

                return CreatedAtAction(nameof(GetArchetypeGenres), new { archetypeId = createdGenre.ArchetypeId }, MapToGenreViewModel(createdGenre));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating archetype genre");
                return StatusCode(500, "An error occurred while creating the archetype genre");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteArchetype(Guid id)
        {
            try
            {
                var result = await _archetypesRepository.DeleteArchetypeAsync(id);
                if (!result)
                {
                    return NotFound($"Archetype with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting archetype with ID {ArchetypeId}", id);
                return StatusCode(500, $"An error occurred while deleting archetype with ID {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMultipleArchetypes([FromBody] List<Guid> archetypeIds)
        {
            try
            {
                if (archetypeIds == null || !archetypeIds.Any())
                    return BadRequest("No archetypes selected for deletion");

                var results = new List<bool>();
                foreach (var id in archetypeIds)
                {
                    var result = await _archetypesRepository.DeleteArchetypeAsync(id);
                    results.Add(result);
                }

                if (results.Any(r => r == true))
                    return Json(new { success = true, deletedCount = results.Count(r => r == true) });
                else
                    return Json(new { success = false, message = "No archetypes were deleted" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple archetypes");
                return StatusCode(500, "An error occurred while deleting archetypes");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteArchetypeGenre(Guid id)
        {
            try
            {
                var result = await _archetypesRepository.DeleteArchetypeGenreAsync(id);
                if (!result)
                {
                    return NotFound($"Archetype genre with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting archetype genre with ID {GenreId}", id);
                return StatusCode(500, $"An error occurred while deleting archetype genre with ID {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveBeats([FromBody] EditBeatsViewModel viewModel)
        {
            try
            {
                var saveBeatsRequest = new SaveBeatsRequestDto
                {
                    ArchetypeId = viewModel.ArchetypeId,
                    Beats = viewModel.Beats.Select(MapToBeatDto).ToList()
                };

                var result = await _archetypesRepository.SaveBeatsAsync(saveBeatsRequest);
                if (!result)
                {
                    return BadRequest("Failed to save beats");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving beats for archetype with ID {ArchetypeId}", viewModel.ArchetypeId);
                return StatusCode(500, $"An error occurred while saving beats for archetype with ID {viewModel.ArchetypeId}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportArchetypes([FromBody] List<Guid> archetypeIds)
        {
            try
            {
                if (archetypeIds == null || !archetypeIds.Any())
                {
                    return BadRequest("No archetypes selected for export");
                }

                var exportData = new List<ArchetypeExportModel>();

                foreach (var id in archetypeIds)
                {
                    var archetypeDto = await _archetypesRepository.GetArchetypeByIdAsync(id);
                    if (archetypeDto == null || archetypeDto.Id == Guid.Empty)
                    {
                        continue; // Skip if archetype not found
                    }

                    var archetypeViewModel = MapToViewModel(archetypeDto);

                    // Get beats for this archetype
                    var beats = await _archetypesRepository.GetArchetypeBeatsByArchetypeIdIdAsync(id);
                    archetypeViewModel.Beats = beats.Select(MapToBeatViewModel).ToList();

                    // Get genres for this archetype
                    var genres = await _archetypesRepository.GetArchetypeGenresByArchetypeIdIdAsync(id);
                    archetypeViewModel.Genres = genres.Select(MapToGenreViewModel).ToList();

                    // Add to export data
                    exportData.Add(new ArchetypeExportModel
                    {
                        Archetype = archetypeViewModel,
                        Beats = archetypeViewModel.Beats,
                        Genres = archetypeViewModel.Genres
                    });
                }

                // Return the export data as JSON
                return Json(new { success = true, data = exportData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting archetypes");
                return StatusCode(500, "An error occurred while exporting archetypes");
            }
        }

        [HttpPost("ImportArchetypes")] // Added explicit route attribute
        public async Task<IActionResult> ImportArchetypes(IFormFile file)
        {
            try
            {
                // Check if a file was actually uploaded and bound
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file provided or file is empty for import");
                    return BadRequest("No file provided for import"); // Keep consistent error message
                }

                // No longer need to get the first file from a list, 'file' is the parameter
                if (file.Length > 0)
                {
                    _logger.LogInformation("Processing import file: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length);

                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        ms.Position = 0;

                        using (var reader = new StreamReader(ms))
                        {
                            var json = await reader.ReadToEndAsync();
                            _logger.LogDebug("JSON content read from file: {ContentLength} characters", json.Length);

                            var archetypes = JsonSerializer.Deserialize<List<ArchetypeExportModel>>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (archetypes == null || !archetypes.Any())
                            {
                                _logger.LogWarning("No archetypes found in the imported file");
                                return BadRequest("No archetypes provided for import");
                            }

                            // Strip IDs from the imported data
                            StripIds(archetypes);

                            _logger.LogInformation("Importing {Count} archetypes", archetypes.Count);

                            var result = await _archetypesRepository.ImportArchetypesAsync(archetypes);
                            if (!result)
                            {
                                _logger.LogWarning("Repository failed to import archetypes");
                                return BadRequest("Failed to import archetypes");
                            }

                            return Ok(new { success = true, message = $"Successfully imported {archetypes.Count} archetypes" });
                        }
                    }
                }
                else
                {
                    return BadRequest("Uploaded file is empty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing archetypes");
                return StatusCode(500, "An error occurred while importing archetypes");
            }
        }

        // Helper method to strip IDs from imported archetypes
        private void StripIds(List<ArchetypeExportModel> archetypes)
        {
            foreach (var item in archetypes)
            {
                // Strip IDs from archetype
                if (item.Archetype != null)
                {
                    item.Archetype.Id = Guid.Empty;
                    item.Archetype.ArchetypeBeatIds = new List<Guid>();
                    item.Archetype.ArchetypeGenreIds = new List<Guid>();

                    // Strip IDs from beats within archetype
                    if (item.Archetype.Beats != null)
                    {
                        foreach (var beat in item.Archetype.Beats)
                        {
                            beat.Id = Guid.Empty;
                            beat.ArchetypeId = Guid.Empty;
                        }
                    }

                    // Strip IDs from genres within archetype
                    if (item.Archetype.Genres != null)
                    {
                        foreach (var genre in item.Archetype.Genres)
                        {
                            genre.Id = Guid.Empty;
                            genre.ArchetypeId = Guid.Empty;
                        }
                    }
                }

                // Strip IDs from beats at top level
                if (item.Beats != null)
                {
                    foreach (var beat in item.Beats)
                    {
                        beat.Id = Guid.Empty;
                        beat.ArchetypeId = Guid.Empty;
                    }
                }

                // Strip IDs from genres at top level
                if (item.Genres != null)
                {
                    foreach (var genre in item.Genres)
                    {
                        genre.Id = Guid.Empty;
                        genre.ArchetypeId = Guid.Empty;
                    }
                }
            }
        }

        #region Mapping Methods

        private ArchetypeViewModel MapToViewModel(ArchetypeDto dto)
        {
            return new ArchetypeViewModel
            {
                Id = dto.Id,
                Domain = dto.Domain,
                Description = dto.Description,
                Architect = dto.Architect,
                ExternalLink = dto.ExternalLink,
                Icon = dto.Icon,
                Rank = dto.Rank,
                ArchetypeBeatIds = dto.ArchetypeBeatIds,
                ArchetypeGenreIds = dto.ArchetypeGenreIds,
                Beats = new List<BeatViewModel>(),
                Genres = new List<GenreViewModel>(),
                // Assuming dto.CreatedUTC is DateTimeKind.Utc or Unspecified but represents UTC
                CreatedDate = new DateTimeOffset(dto.CreatedUTC.Kind == DateTimeKind.Unspecified
                                                ? DateTime.SpecifyKind(dto.CreatedUTC, DateTimeKind.Utc)
                                                : dto.CreatedUTC)
            };
        }

        private BeatViewModel MapToBeatViewModel(ArchetypeBeatDto dto)
        {
            return new BeatViewModel
            {
                Id = dto.Id,
                ArchetypeId = dto.ArchetypeId,
                ParentSequence = dto.ParentSequence,
                ChildSequence = dto.ChildSequence,
                GrandchildSequence = dto.GrandchildSequence,
                Name = dto.Name,
                Description = dto.Description,
                Prompt = dto.Prompt,
                PercentOfStory = dto.PercentOfStory
            };
        }

        private GenreViewModel MapToGenreViewModel(ArchetypeGenreDto dto)
        {
            return new GenreViewModel
            {
                Id = dto.Id,
                ArchetypeId = dto.ArchetypeId,
                Name = dto.Name,
                Description = dto.Description
            };
        }

        private ArchetypeDto MapToDto(ArchetypeViewModel viewModel)
        {
            return new ArchetypeDto
            {
                Id = viewModel.Id,
                Domain = viewModel.Domain,
                Description = viewModel.Description,
                Architect = viewModel.Architect,
                ExternalLink = viewModel.ExternalLink,
                Icon = viewModel.Icon,
                Rank = viewModel.Rank,
                ArchetypeBeatIds = viewModel.ArchetypeBeatIds,
                ArchetypeGenreIds = viewModel.ArchetypeGenreIds
            };
        }

        private ArchetypeBeatDto MapToBeatDto(BeatViewModel viewModel)
        {
            return new ArchetypeBeatDto
            {
                Id = viewModel.Id,
                ArchetypeId = viewModel.ArchetypeId,
                ParentSequence = viewModel.ParentSequence,
                ChildSequence = viewModel.ChildSequence,
                GrandchildSequence = viewModel.GrandchildSequence,
                Name = viewModel.Name,
                Description = viewModel.Description,
                Prompt = viewModel.Prompt,
                PercentOfStory = viewModel.PercentOfStory
            };
        }

        private ArchetypeGenreDto MapToDto(GenreViewModel viewModel)
        {
            return new ArchetypeGenreDto
            {
                Id = viewModel.Id,
                ArchetypeId = viewModel.ArchetypeId,
                Name = viewModel.Name,
                Description = viewModel.Description
            };
        }

        #endregion
    }
}