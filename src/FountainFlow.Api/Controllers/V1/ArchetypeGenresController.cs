using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FountainFlow.Api.Entities;
using FountainFlow.Api.Data;
using FountainFlow.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FountainFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ArchetypeGenresController : ControllerBase
    {
        private readonly FFDbContext _ffDbContext;
        private readonly IMapper _mapper;

        public ArchetypeGenresController(FFDbContext ffDbContext, IMapper mapper)
        {
            _ffDbContext = ffDbContext;
            _mapper = mapper;
        }

        // GET: api/ArchetypeGenres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArchetypeGenreReadDto>>> GetArchetypeGenres()
        {
            var archetypeGenres = await _ffDbContext.ArchetypeGenres.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ArchetypeGenreReadDto>>(archetypeGenres));
        }

        // GET: api/ArchetypeGenres/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ArchetypeGenreReadDto>> GetArchetypeGenre(Guid id)
        {
            var archetypeGenre = await _ffDbContext.ArchetypeGenres.FirstOrDefaultAsync(ag => ag.Id == id);

            if (archetypeGenre == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ArchetypeGenreReadDto>(archetypeGenre));
        }

        // POST: api/ArchetypeGenres
        [HttpPost]
        public async Task<ActionResult<ArchetypeGenreReadDto>> CreateArchetypeGenre(ArchetypeGenreDto archetypeGenreDto)
        {
            var archetypeGenre = _mapper.Map<ArchetypeGenre>(archetypeGenreDto);
            archetypeGenre.Id = Guid.NewGuid();
            archetypeGenre.CreatedUTC = DateTime.UtcNow;
            archetypeGenre.UpdatedUTC = DateTime.UtcNow;
            _ffDbContext.ArchetypeGenres.Add(archetypeGenre);
            await _ffDbContext.SaveChangesAsync();

            var archetypeGenreReadDto = _mapper.Map<ArchetypeGenreReadDto>(archetypeGenre);
            return CreatedAtAction(nameof(GetArchetypeGenre), new { id = archetypeGenre.Id }, archetypeGenreReadDto);
        }

        // PUT: api/ArchetypeGenres/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArchetypeGenre(Guid id, ArchetypeGenreDto archetypeGenreDto)
        {
            if (id != archetypeGenreDto.Id)
            {
                return BadRequest();
            }

            var archetypeGenre = await _ffDbContext.ArchetypeGenres.FindAsync(id);
            if (archetypeGenre == null)
            {
                return NotFound();
            }

            _mapper.Map(archetypeGenreDto, archetypeGenre);
            archetypeGenre.UpdatedUTC = DateTime.UtcNow;

            try
            {
                await _ffDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArchetypeGenreExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ArchetypeGenres/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArchetypeGenre(Guid id)
        {
            var archetypeGenre = await _ffDbContext.ArchetypeGenres.FindAsync(id);
            if (archetypeGenre == null)
            {
                return NotFound();
            }

            _ffDbContext.ArchetypeGenres.Remove(archetypeGenre);
            await _ffDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ArchetypeGenreExists(Guid id)
        {
            return _ffDbContext.ArchetypeGenres.Any(e => e.Id == id);
        }
    }
}