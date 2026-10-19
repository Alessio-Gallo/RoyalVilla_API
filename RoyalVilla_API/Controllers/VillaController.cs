using System.Net;
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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
    {
        var villas = await _db.Villa.ToListAsync(); 
        var responseData = _mapper.Map<List<VillaDTO>>(villas); 
        var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(
            responseData, "Villas retrieved successfully"
        );

        return Ok(response); 
    } 


    // Get a record
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
    {
        try
        {
            if(id <= 0) 
            {
                return BadRequest(ApiResponse<VillaDTO>.BadRequest($"Villa ID must be greater than 0"));
            }

            var villa = await _db.Villa.FindAsync(id);
            if(villa == null)
            {
                return NotFound(ApiResponse<VillaDTO>.NotFound($"Villa with ID='{id}' was not found"));
            }

            return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Record retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<VillaDTO>.Error(HttpStatusCode.InternalServerError, 
                $"An error occurred while retrieving villa with ID {id}:",
                ex.Message
            ));
        }
    }


    // Create a record
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaDTO)
    {
        try
        {
            if(villaDTO == null)
            {
                return BadRequest(ApiResponse<VillaDTO>.BadRequest("Villa data is required"));
            }

            // Check for dups
            var isDuplicate = await _db.Villa
                .AnyAsync(v => v.Name.ToLower() == villaDTO.Name.ToLower());

            if(isDuplicate)
            {
                return Conflict(
                    ApiResponse<VillaDTO>.Conflict($"A villa with the name: '{villaDTO.Name}' already exists")
                );
            }

            // Explicit cast to Villa using AutoMapper
            Villa villa = _mapper.Map<Villa>(villaDTO);
            villa.CreatedDate = DateTime.UtcNow; 

            await _db.Villa.AddAsync(villa);
            await _db.SaveChangesAsync();

            var response = ApiResponse<VillaDTO>.CreatedAt(
                _mapper.Map<VillaDTO>(villa), "Villa created successfully"
            );

            return CreatedAtAction(nameof(GetVillaById), new { id = villa.Id }, response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<VillaDTO>.Error(
                HttpStatusCode.InternalServerError,
                $"An error occurred while creating the villa: ",
                ex.Message
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }


    // Update a record
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, VillaUpdateDTO villaUpdateDTO)
    {
        try
        {
            if (id != villaUpdateDTO.Id)
            {
                return BadRequest(ApiResponse<VillaDTO>.BadRequest(
                    "Villa ID in the URL does not match Villa ID in request body")
                );
            }

            var existingVilla = await _db.Villa.FindAsync(id);

            if (existingVilla == null)
            {
                return NotFound(ApiResponse<VillaDTO>.NotFound(
                    $"Villa with ID='{id}' was not found")
                );
            }

            // Check for dups
            var isDuplicate = await _db.Villa
                .AnyAsync(v => v.Id != id && v.Name.ToLower() == villaUpdateDTO.Name.ToLower());

            if (isDuplicate)
            {
                return Conflict(ApiResponse<VillaDTO>.Conflict(
                    $"A villa with the name: '{villaUpdateDTO.Name}' already exists"
                ));
            }

            _mapper.Map(villaUpdateDTO, existingVilla);
            existingVilla.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(ApiResponse<VillaDTO>.Ok(
                _mapper.Map<VillaDTO>(existingVilla), "Villa updated successfully"
            ));
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<VillaDTO>.Error(
                HttpStatusCode.InternalServerError,
                "An error occurred while updating the villa",
                ex.Message
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // Delete a record
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
    {
        try
        {
            var existingVilla = await _db.Villa.FindAsync(id);

            if(existingVilla == null) {
                return NotFound(ApiResponse<object>.NotFound($"Villa with id:{id} was not found"));
            }

            _db.Villa.Remove(existingVilla);
            await _db.SaveChangesAsync();

            var response = ApiResponse<object>.NoContent("Villa deleted successfully");
            return Ok(response); 
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<object>.Error(
                HttpStatusCode.InternalServerError,
                $"An error occurred while deleting the villa: ",
                ex.Message
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}

