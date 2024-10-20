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
    public class ArchetypeBeatsController : ControllerBase
    {
        private readonly FFDbContext _ffDbContext;
        private readonly IMapper _mapper;

        public ArchetypeBeatsController(FFDbContext ffDbContext, IMapper mapper)
        {
            _ffDbContext = ffDbContext;
            _mapper = mapper;
        }

        // GET: api/ArchetypeBeats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArchetypeBeatReadDto>>> GetArchetypeBeats()
        {
            var archetypeBeats = await _ffDbContext.ArchetypeBeats.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ArchetypeBeatReadDto>>(archetypeBeats));
        }

        // GET: api/ArchetypeBeats/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ArchetypeBeatReadDto>> GetArchetypeBeat(Guid id)
        {
            var archetypeBeat = await _ffDbContext.ArchetypeBeats.FirstOrDefaultAsync(ab => ab.Id == id);

            if (archetypeBeat == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ArchetypeBeatReadDto>(archetypeBeat));
        }

        // POST: api/ArchetypeBeats
        [HttpPost]
        public async Task<ActionResult<ArchetypeBeatReadDto>> CreateArchetypeBeat(ArchetypeBeatDto archetypeBeatDto)
        {
            var archetypeBeat = _mapper.Map<ArchetypeBeat>(archetypeBeatDto);
            archetypeBeat.Id = Guid.NewGuid();
            archetypeBeat.CreatedUTC = DateTime.UtcNow;
            archetypeBeat.UpdatedUTC = DateTime.UtcNow;
            _ffDbContext.ArchetypeBeats.Add(archetypeBeat);
            await _ffDbContext.SaveChangesAsync();

            var archetypeBeatReadDto = _mapper.Map<ArchetypeBeatReadDto>(archetypeBeat);
            return CreatedAtAction(nameof(GetArchetypeBeat), new { id = archetypeBeat.Id }, archetypeBeatReadDto);
        }

        // PUT: api/ArchetypeBeats/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArchetypeBeat(Guid id, ArchetypeBeatDto archetypeBeatDto)
        {
            if (id != archetypeBeatDto.Id)
            {
                return BadRequest();
            }

            var archetypeBeat = await _ffDbContext.ArchetypeBeats.FindAsync(id);
            if (archetypeBeat == null)
            {
                return NotFound();
            }

            _mapper.Map(archetypeBeatDto, archetypeBeat);
            archetypeBeat.UpdatedUTC = DateTime.UtcNow;

            try
            {
                await _ffDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArchetypeBeatExists(id))
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

        // DELETE: api/ArchetypeBeats/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArchetypeBeat(Guid id)
        {
            var archetypeBeat = await _ffDbContext.ArchetypeBeats.FindAsync(id);
            if (archetypeBeat == null)
            {
                return NotFound();
            }

            _ffDbContext.ArchetypeBeats.Remove(archetypeBeat);
            await _ffDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ArchetypeBeatExists(Guid id)
        {
            return _ffDbContext.ArchetypeBeats.Any(e => e.Id == id);
        }
    }
}