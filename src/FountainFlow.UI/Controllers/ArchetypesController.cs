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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FountainFlowUI.Controllers
{
    public class ArchetypesController : Controller
    {
        private readonly ILogger<ArchetypesController> _logger;
        private readonly IArchetypesRepository _archetypesRepository;

        public ArchetypesController(ILogger<ArchetypesController> logger, IArchetypesRepository archetypesRepository)
        {
            _logger = logger;
            _archetypesRepository = archetypesRepository;
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
        public async Task<IActionResult> CreateArchetype([FromBody] ArchetypeViewModel viewModel)
        {
            try
            {
                var archetypeDto = MapToDto(viewModel);
                var createdArchetype = await _archetypesRepository.CreateArchetypeAsync(archetypeDto);

                return CreatedAtAction(nameof(GetArchetype), new { id = createdArchetype.Id }, MapToViewModel(createdArchetype));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating archetype");
                return StatusCode(500, "An error occurred while creating the archetype");
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

        [HttpPost]
        public async Task<IActionResult> ImportArchetypes(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    _logger.LogWarning("No files provided for import");
                    return BadRequest("No files provided for import");
                }

                var file = files[0];
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
                Genres = new List<GenreViewModel>()
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