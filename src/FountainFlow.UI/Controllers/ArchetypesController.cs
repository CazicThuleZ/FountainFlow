using FountainFlowUI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FountainFlowUI.Controllers;

public class ArchetypesController : Controller
{
    private readonly ILogger<ArchetypesController> _logger;
    private readonly IArchetypesRepository _archetypesRepository;
    public ArchetypesController(ILogger<ArchetypesController> logger, IArchetypesRepository archetypesRepository)
    {
        _logger = logger;
        _archetypesRepository = archetypesRepository;
    }

    public IActionResult Archetypes()
    {
        return View();
    }
}
