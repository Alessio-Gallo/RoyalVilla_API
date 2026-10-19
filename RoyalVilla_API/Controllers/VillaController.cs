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

    // Get all the records
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Villa>>> GetVillas()
    {
        return Ok(await _db.Villa.ToListAsync()); 
    } 

    // Get a record
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
                return NotFound($"Villa with ID='{id}' was not found");
            }

            return Ok(villa); 
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                $"An error occurred while retrieving villa with ID {id}: {ex.Message}");
        }
    }

    // Create a record
    [HttpPost]
    public async Task<ActionResult<Villa>> CreateVilla(VillaCreateDTO villaDTO)
    {
        try
        {
            // Explicit cast to Villa using AutoMapper
            Villa villa = _mapper.Map<Villa>(villaDTO);
            villa.CreatedDate = DateTime.UtcNow; 

            await _db.Villa.AddAsync(villa);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, villa);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
            $"An error occurred while creating the villa: {ex.Message}") ;
        }
    }

    // Update a record
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Villa>> UpdateVilla([FromRoute] int id, VillaUpdateDTO villaDTO)
    {
        try
        {
            if(id != villaDTO.Id)
            {
                return BadRequest("Villa ID in the URL does not match Villa ID in request body");
            }

            var existingVilla = await _db.Villa.FindAsync(id);

            if(existingVilla == null)
            {
                return NotFound($"Villa with ID='{id}' was not found");
            }

            // Check for dups
            var isDuplicate = await _db.Villa
                .AnyAsync(v => v.Id != id && v.Name.ToLower() == villaDTO.Name.ToLower());

            if(isDuplicate)
            {   // 409 error code
                return Conflict($"A villa with the name: '{villaDTO.Name}' already exists");
            }

            _mapper.Map(villaDTO, existingVilla); 
            existingVilla.UpdateDate = DateTime.UtcNow; 

            await _db.SaveChangesAsync();

            return Ok(villaDTO);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while updating the villa: {ex.Message}");
        } 
    }

    // Delete a record
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Villa>> DeleteVilla(int id)
    {
        try
        {
            var existingVilla = await _db.Villa.FindAsync(id);

            if(existingVilla == null) {
                return NotFound($"Villa with id:{id} was not found");
            }

            _db.Villa.Remove(existingVilla);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                $"An error occurred while deleting the villa:{ex.Message}");
        }
        return NoContent(); 
    }
}

