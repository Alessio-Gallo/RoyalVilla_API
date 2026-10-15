using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers;

[Route("api/villa")]
[ApiController]
public class VillaController(ApplicationDbContext db, IMapper mapper) : ControllerBase
{
    private readonly ApplicationDbContext _db = db;
    private readonly IMapper _mapper = mapper; 

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Villa>>> GetVillas()
    {
        return Ok(await _db.Villa.ToListAsync()); 
    } 

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Villa>> GetVillaById(int id)
    {
        try
        {
            if(id <= 0) {
                return BadRequest("Villa ID must be greater than 0");
            }

            var villa = await _db.Villa.FindAsync(id);
            if(villa == null)
            {
                return NotFound($"Villa with ID={id} was not found");
            }

            return Ok(villa); 
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                $"An error occurred while retrieving villa with ID {id}: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Villa>> CreateVilla(VillaCreateDTO villaDTO)
    {
        try
        {
            // Explicit cast to Villa using AutoMapper
            Villa villa = _mapper.Map<Villa>(villaDTO);
            villa.CreatedDate = DateTime.UtcNow; 

            _db.Villa.Add(villa);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, villa);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
            $"An error occurred while creating the villa: {ex.Message}") ;
        }
    }
}

