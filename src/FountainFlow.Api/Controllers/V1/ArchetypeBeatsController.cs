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

        // GET: api/ArchetypeBeats/archetype/{id}
        [HttpGet("archetype/{id}")]
        public async Task<ActionResult<List<ArchetypeBeatReadDto>>> GetArchetypeBeatsByArchetypeId(Guid id)
        {
            var archetypeBeats = await _ffDbContext.ArchetypeBeats
                .Where(ab => ab.ArchetypeId == id)
                .OrderBy(ab => ab.Sequence)
                .ToListAsync();

            if (!archetypeBeats.Any())
                return NotFound();

            var archetypeBeatDtos = _mapper.Map<List<ArchetypeBeatReadDto>>(archetypeBeats);
            return Ok(archetypeBeatDtos);
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

        [HttpPost("SaveBeats")]
        public async Task<IActionResult> SaveBeats([FromBody] SaveBeatsRequestDto request)
        {
            if (request == null || request.Beats == null)
                return BadRequest("Invalid request data.");

            try
            {
                var existingBeats = await _ffDbContext.ArchetypeBeats
                    .Where(ab => ab.ArchetypeId == request.ArchetypeId)
                    .ToListAsync();

                var beatsToDelete = existingBeats
                    .Where(eb => !request.Beats.Any(rb => rb.Id == eb.Id))
                    .ToList();

                var beatsToUpdate = existingBeats
                    .Where(eb => request.Beats.Any(rb => rb.Id == eb.Id))
                    .ToList();

                var beatsToAdd = request.Beats
                    .Where(rb => !existingBeats.Any(eb => eb.Id == rb.Id))
                    .Select(rb => new ArchetypeBeat
                    {
                        Id = Guid.NewGuid(),
                        ArchetypeId = request.ArchetypeId,
                        Name = rb.Name,
                        Description = rb.Description,
                        Sequence = rb.Sequence,
                        PercentOfStory = rb.PercentOfStory,
                        CreatedUTC = DateTime.UtcNow,
                        UpdatedUTC = DateTime.UtcNow
                    })
                    .ToList();

                // Perform delete, update, and add operations
                if (beatsToDelete.Any())
                {
                    _ffDbContext.ArchetypeBeats.RemoveRange(beatsToDelete);
                }

                foreach (var existingBeat in beatsToUpdate)
                {
                    var updatedBeat = request.Beats.First(rb => rb.Id == existingBeat.Id);
                    existingBeat.Name = updatedBeat.Name;
                    existingBeat.Description = updatedBeat.Description;
                    existingBeat.Sequence = updatedBeat.Sequence;
                    existingBeat.PercentOfStory = updatedBeat.PercentOfStory;
                    existingBeat.UpdatedUTC = DateTime.UtcNow;
                }

                if (beatsToAdd.Any())
                {
                    _ffDbContext.ArchetypeBeats.AddRange(beatsToAdd);
                }

                // Save changes to the database
                await _ffDbContext.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while saving beats:  " + ex.Message);
            }
        }


        private bool ArchetypeBeatExists(Guid id)
        {
            return _ffDbContext.ArchetypeBeats.Any(e => e.Id == id);
        }
    }
}