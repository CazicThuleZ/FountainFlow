using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FountainFlow.Api.DTOs;
using FountainFlow.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FountainFlow.Api.Data;

namespace FountainFlow.Api.Controllers
{
    [ApiController]
    [Route("api/archetype")]
    public class ArchetypesController : ControllerBase
    {
        private readonly FFDbContext _context;
        private readonly IMapper _mapper;

        public ArchetypesController(FFDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Archetypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArchetypeReadDto>>> GetArchetypes()
        {
            var archetypes = await _context.Archetypes
                .Include(a => a.ArchetypeBeats)
                .Include(a => a.ArchetypeGenres)
                .ToListAsync();

            var archetypeDtos = _mapper.Map<IEnumerable<ArchetypeReadDto>>(archetypes);
            return Ok(archetypeDtos);
        }

        // GET: api/Archetypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ArchetypeReadDto>> GetArchetype(Guid id)
        {
            var archetype = await _context.Archetypes
                .Include(a => a.ArchetypeBeats)
                .Include(a => a.ArchetypeGenres)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (archetype == null)
            {
                return NotFound();
            }

            var archetypeDto = _mapper.Map<ArchetypeReadDto>(archetype);
            return Ok(archetypeDto);
        }

        // PUT: api/Archetypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArchetype(Guid id, ArchetypeDto archetypeDto)
        {
            if (id != archetypeDto.Id)
                return BadRequest();

            var archetype = await _context.Archetypes
                .Include(a => a.ArchetypeBeats)
                .Include(a => a.ArchetypeGenres)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (archetype == null)
            {
                return NotFound();
            }

            _mapper.Map(archetypeDto, archetype);
            archetype.UpdatedUTC = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
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

        // POST: api/Archetypes
        [HttpPost]
        public async Task<ActionResult<ArchetypeReadDto>> PostArchetype(ArchetypeDto archetypeDto)
        {
            var archetype = _mapper.Map<Archetype>(archetypeDto);
            archetype.Id = Guid.NewGuid();
            archetype.CreatedUTC = DateTime.UtcNow;
            archetype.UpdatedUTC = DateTime.UtcNow;

            _context.Archetypes.Add(archetype);
            await _context.SaveChangesAsync();

            var archetypeReadDto = _mapper.Map<ArchetypeReadDto>(archetype);
            return CreatedAtAction(nameof(GetArchetype), new { id = archetypeReadDto.Id }, archetypeReadDto);
        }

        // DELETE: api/Archetypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArchetype(Guid id)
        {
            var archetype = await _context.Archetypes.FindAsync(id);
            if (archetype == null)
            {
                return NotFound();
            }

            _context.Archetypes.Remove(archetype);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArchetypeExists(Guid id)
        {
            return _context.Archetypes.Any(e => e.Id == id);
        }
    }
}
