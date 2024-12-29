using FountainFlow.UI.DTOs;
using FountainFlow.UI.Models;
using FountainFlowUI.DTOs;
using FountainFlowUI.Interfaces;
using FountainFlowUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FountainFlowUI.Controllers;

public class ArchetypesController : Controller
{
    private readonly ILogger<ArchetypesController> _logger;
    private readonly IArchetypesRepository _archetypesRepository;
    public ArchetypesController(ILogger<ArchetypesController> logger, IArchetypesRepository archetypesRepository)
    {
        _archetypesRepository = archetypesRepository ?? throw new ArgumentNullException(nameof(archetypesRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    public async Task<IActionResult> Archetypes()
    {
        try
        {
            _logger.LogInformation("Fetching archetypes from repository");

            var archetypes = await _archetypesRepository.GetArchetypesAsync();

            if (archetypes == null || !archetypes.Any())
            {
                _logger.LogWarning("No archetypes found");
                return View(new ArchetypeViewModel { Archetypes = new List<ArchetypeViewModel>() });
            }

            var model = new ArchetypeViewModel
            {
                Archetypes = archetypes.Select(a => new ArchetypeViewModel
                {
                    Id = a.Id,
                    Domain = a.Domain ?? string.Empty,
                    Description = a.Description ?? string.Empty,
                    Architect = a.Architect ?? string.Empty,
                    ExternalLink = a.ExternalLink ?? string.Empty,
                    Icon = a.Icon ?? string.Empty,
                    ArchetypeBeatIds = a.ArchetypeBeatIds ?? new List<Guid>(),
                    ArchetypeGenreIds = a.ArchetypeGenreIds ?? new List<Guid>(),
                    Beats = new List<BeatViewModel>(),
                    Genres = new List<GenreViewModel>()
                }).ToList()
            };

            foreach (var archetype in model.Archetypes)
            {
                var genresTask = _archetypesRepository.GetArchetypeGenresByArchetypeIdIdAsync(archetype.Id);
                var beatsTask = _archetypesRepository.GetArchetypeBeatsByArchetypeIdIdAsync(archetype.Id);

                var genres = await genresTask;
                var beats = await beatsTask;

                if (genres != null && genres.Any())
                {
                    archetype.Genres.AddRange(genres.Select(genre => new GenreViewModel
                    {
                        Id = genre.Id,
                        Name = genre.Name,
                        Description = genre.Description,
                        ArchetypeId = archetype.Id
                    }));
                }

                if (beats != null && beats.Any())
                {
                    archetype.Beats.AddRange(beats.Select(beat => new BeatViewModel
                    {
                        Id = beat.Id,
                        Name = beat.Name,
                        Description = beat.Description,
                        ArchetypeId = archetype.Id
                    }));
                }
            }

            _logger.LogInformation("Successfully mapped {Count} archetypes to view model", model.Archetypes.Count);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching or mapping archetypes");
            return View("Error", new ErrorViewModel
            {
                Message = "Unable to load archetypes at this time. Please try again later."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetArchetypeDetails(Guid id)
    {
        try
        {
            _logger.LogInformation("GetArchetypeDetails called with id: {Id}", id);
            var archetype = await _archetypesRepository.GetArchetypeByIdAsync(id);

            if (archetype == null)
            {
                _logger.LogWarning("Archetype not found for id: {Id}", id);
                return NotFound();
            }

            var model = new ArchetypeViewModel
            {
                Id = archetype.Id,
                Domain = archetype.Domain,
                Description = archetype.Description,
                Architect = archetype.Architect,
                ExternalLink = archetype.ExternalLink,
                Icon = archetype.Icon,
                ArchetypeBeatIds = archetype.ArchetypeBeatIds,
                ArchetypeGenreIds = archetype.ArchetypeGenreIds,
                Beats = new List<BeatViewModel>(),
                Genres = new List<GenreViewModel>()
            };

            var genresTask = _archetypesRepository.GetArchetypeGenresByArchetypeIdIdAsync(model.Id);
            var beatsTask = _archetypesRepository.GetArchetypeBeatsByArchetypeIdIdAsync(model.Id);

            var genres = await genresTask;
            var beats = await beatsTask;

            model.Genres.AddRange(genres.Select(genre => new GenreViewModel
            {
                Id = genre.Id,
                Name = genre.Name,
                Description = genre.Description,
                ArchetypeId = model.Id
            }));

            model.Beats.AddRange(beats.Select(beat => new BeatViewModel
            {
                Id = beat.Id,
                Name = beat.Name,
                Description = beat.Description,
                ArchetypeId = model.Id
            }));

            return PartialView("_ArchetypeDetailsPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching archetype");
            return View("Error", new ErrorViewModel
            {
                Message = "Unable to retreive archetype at this time. Please try again later."
            });
        }
    }

    [HttpDelete("DeleteArchetype")]
    public async Task<IActionResult> DeleteArchetype(Guid id)
    {
        try
        {
            var result = await _archetypesRepository.DeleteArchetypeAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting archetype");
            return View("Error", new ErrorViewModel
            {
                Message = "Unable to delete archetype at this time. Please try again later."
            });
        }
    }

    [HttpGet]
    public IActionResult CreateArchetype()
    {
        // Return a partial view for the create form
        return PartialView("_CreateArchetypePartial", new ArchetypeViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateArchetype([FromBody] ArchetypeViewModel model)
    {
        _logger.LogInformation("Received model: {@Model}", model);

        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new archetype with domain: {Domain}", model.Domain);

            // Map view model to DTO
            var archetypeDto = new ArchetypeDto
            {
                Domain = model.Domain,
                Description = model.Description,
                Architect = model.Architect,
                ExternalLink = model.ExternalLink,
                Icon = model.Icon,
                ArchetypeBeatIds = model.ArchetypeBeatIds ?? new List<Guid>(),
                ArchetypeGenreIds = model.ArchetypeGenreIds ?? new List<Guid>()
            };

            var result = await _archetypesRepository.CreateArchetypeAsync(archetypeDto);

            if (result == null)
            {
                _logger.LogWarning("Failed to create archetype");
                return BadRequest("Failed to create archetype");
            }

            // Map the created archetype back to a view model
            var createdModel = new ArchetypeViewModel
            {
                Id = result.Id,
                Domain = result.Domain,
                Description = result.Description,
                Architect = result.Architect,
                ExternalLink = result.ExternalLink,
                Icon = result.Icon,
                ArchetypeBeatIds = result.ArchetypeBeatIds,
                ArchetypeGenreIds = result.ArchetypeGenreIds,
                Beats = new List<BeatViewModel>(),
                Genres = new List<GenreViewModel>()
            };

            return Json(createdModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating archetype");
            return BadRequest("Unable to create archetype at this time. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadIcon(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        try
        {
            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images"));

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new { fileName = fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading icon");
            return BadRequest("Failed to upload file");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateGenre([FromBody] ArchetypeGenreDto genreDto)
    {
        try
        {
            _logger.LogInformation("XReceived genre creation request: {@GenreDto}", genreDto);
            var result = await _archetypesRepository.CreateArchetypeGenreAsync(genreDto);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating genre: {Message}", ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("DeleteGenre")]
    public async Task<IActionResult> DeleteGenre(Guid id)
    {
        try
        {
            var result = await _archetypesRepository.DeleteArchetypeGenreAsync(id);
            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting genre");
            return Json(new
            {
                success = false,
                message = "Unable to delete genre at this time. Please try again later."
            });
        }
    }

    [HttpGet]
    [Route("Archetypes/EditBeats/{id}")]
    public async Task<IActionResult> EditBeats(Guid id, string domain)
    {
        try
        {
            var beats = await _archetypesRepository.GetArchetypeBeatsByArchetypeIdIdAsync(id);

            var model = new EditBeatsViewModel
            {
                ArchetypeId = id,
                Domain = domain,
                Beats = beats.Select(b => new BeatViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Sequence = b.Sequence,
                    PercentOfStory = b.PercentOfStory,
                    ArchetypeId = id
                }).OrderBy(b => b.Sequence).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching beats for archetype {Id}", id);
            return View("Error", new ErrorViewModel
            {
                Message = "Unable to load beats at this time. Please try again later."
            });
        }
    }

    [HttpPost]
    [Route("Archetypes/SaveBeats")]
    public async Task<IActionResult> SaveBeats([FromBody] SaveBeatsRequest request)
    {
        try
        {
            // TODO: Implement your save logic here
            // You'll want to handle both updates to existing beats
            // and creation of new beats (those with temp_ ids)

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving beats");
            return Json(new { success = false, message = "Failed to save changes" });
        }
    }
}