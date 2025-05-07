using Microsoft.AspNetCore.Mvc;
using kolos1.Services;
using kolos1.Exceptions;
using kolos1.Models.DTOs;

namespace kolos1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitsController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        // GET api/visits/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(int id)
        {
            try
            {
                var visit = await _visitService.GetVisitByIdAsync(id);
                return Ok(visit); // Ok (status 200)
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message); // NotFound (status 404)
            }
        }

        // POST api/visits
        [HttpPost]
        public async Task<IActionResult> CreateVisit([FromBody] CreateVisitDto visitDto)
        {
            if (visitDto.Services.Count == 0)
            {
                return BadRequest("At least one service is required."); // BadRequest (status 400)
            }

            try
            {
                await _visitService.CreateVisitAsync(visitDto);
                return CreatedAtAction(nameof(GetVisit), new { id = visitDto.VisitId }, visitDto); // CreatedAtAction (status 201)
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message); // NotFound (status 404)
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message); // Conflict (status 409)
            }
        }
    }
}