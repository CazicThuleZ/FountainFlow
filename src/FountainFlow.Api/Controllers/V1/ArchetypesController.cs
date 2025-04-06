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
    public class ArchetypesController : ControllerBase
    {
        private readonly FFDbContext _ffDbContext;
        private readonly IMapper _mapper;

        public ArchetypesController(FFDbContext ffDbContext, IMapper mapper)
        {
            _ffDbContext = ffDbContext;
            _mapper = mapper;
        }

        // GET: api/Archetypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArchetypeReadDto>>> GetArchetypes()
        {
            var archetypes = await _ffDbContext.Archetypes
                .Include(a => a.ArchetypeBeats)
                .Include(a => a.ArchetypeGenres)
                .OrderBy(a => a.Rank)  // Sort by Rank
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ArchetypeReadDto>>(archetypes));
        }

        // GET: api/Archetypes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ArchetypeReadDto>> GetArchetype(Guid id)
        {
            var archetype = await _ffDbContext.Archetypes.Include(a => a.ArchetypeBeats).Include(a => a.ArchetypeGenres).FirstOrDefaultAsync(a => a.Id == id);

            if (archetype == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ArchetypeReadDto>(archetype));
        }

        // POST: api/Archetypes
        [HttpPost]
        public async Task<ActionResult<ArchetypeReadDto>> CreateArchetype(ArchetypeDto archetypeDto)
        {
            var archetype = _mapper.Map<Archetype>(archetypeDto);
            archetype.Id = Guid.NewGuid();
            archetype.CreatedUTC = DateTime.UtcNow;
            archetype.UpdatedUTC = DateTime.UtcNow;
            _ffDbContext.Archetypes.Add(archetype);
            await _ffDbContext.SaveChangesAsync();

            var archetypeReadDto = _mapper.Map<ArchetypeReadDto>(archetype);
            return CreatedAtAction(nameof(GetArchetype), new { id = archetype.Id }, archetypeReadDto);
        }

        // PUT: api/Archetypes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArchetype(Guid id, ArchetypeDto archetypeDto)
        {
            if (id != archetypeDto.Id)
            {
                return BadRequest();
            }

            var archetype = await _ffDbContext.Archetypes.FindAsync(id);
            if (archetype == null)
            {
                return NotFound();
            }

            _mapper.Map(archetypeDto, archetype);
            archetype.UpdatedUTC = DateTime.UtcNow;

            try
            {
                await _ffDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArchetypeExists(id))
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

        // DELETE: api/Archetypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArchetype(Guid id)
        {
            var archetype = await _ffDbContext.Archetypes.FindAsync(id);
            if (archetype == null)
            {
                return NotFound();
            }

            _ffDbContext.Archetypes.Remove(archetype);
            await _ffDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ArchetypeExists(Guid id)
        {
            return _ffDbContext.Archetypes.Any(e => e.Id == id);
        }
    }
}