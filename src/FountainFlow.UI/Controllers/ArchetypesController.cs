using FountainFlow.UI.Models;
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

                    }));
                }

                if (beats != null && beats.Any())
                {
                    archetype.Beats.AddRange(beats.Select(beat => new BeatViewModel
                    {
                        Id = beat.Id,
                        Name = beat.Name,
                        Description = beat.Description
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
            }));

            model.Beats.AddRange(beats.Select(beat => new BeatViewModel
            {
                Id = beat.Id,
                Name = beat.Name,
                Description = beat.Description
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

    [HttpDelete]
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
}