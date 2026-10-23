using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers;

[Route("api/villa")]
[ApiController]
// [Authorize(Roles = "Customer,Admin")]
public class VillaController(ApplicationDbContext dbContext, IMapper mapper) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper; 

    // Get all the records
    [HttpGet]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
    {
        var villas = await _dbContext.Villa
            .AsNoTracking()
            .ToListAsync();
        
        var responseData = _mapper.Map<List<VillaDTO>>(villas); 
        var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(
            responseData, "Villas retrieved successfully"
        );

        return Ok(response); 
    } 


    // Get a record
    [HttpGet("{id:int}")]
    // [AllowAnonymous]
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
                return BadRequest(ApiResponse<VillaDTO>.BadRequest("Villa ID must be greater than 0"));
            }

            var villa = await _dbContext.Villa.FindAsync(id);
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
                $"An error occurred while retrieving villa with ID {id}",
                ex.Message
            ));
        }
    }


    // Create a record
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO dto)  
    {
        try
        {
            // Check for dups
            var isDuplicate = await _dbContext.Villa
                .AnyAsync(v => v.Name.ToLower() == dto.Name.ToLower());

            if(isDuplicate)
            {
                return Conflict(
                    ApiResponse<VillaDTO>.Conflict($"A villa with the name: '{dto.Name}' already exists")
                );
            }

            // Explicit cast to Villa using AutoMapper
            Villa villa = _mapper.Map<Villa>(dto);
            villa.CreatedDate = DateTime.UtcNow; 

            _dbContext.Villa.Add(villa);
            await _dbContext.SaveChangesAsync();

            var response = ApiResponse<VillaDTO>.CreatedAt(
                _mapper.Map<VillaDTO>(villa), "Villa created successfully"
            );

            return CreatedAtAction(nameof(GetVillaById), new { id = villa.Id }, response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<VillaDTO>.Error(
                HttpStatusCode.InternalServerError,
                $"An error occurred while creating the villa",
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
    public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, VillaUpdateDTO dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<VillaDTO>.BadRequest(
                    "Villa ID in the URL does not match Villa ID in the request body")
                );
            }

            var existingVilla = await _dbContext.Villa.FindAsync(id);

            if (existingVilla == null)
            {
                return NotFound(ApiResponse<VillaDTO>.NotFound(
                    $"Villa with ID='{id}' was not found")
                );
            }

            // Check for dups
            var isDuplicate = await _dbContext.Villa
                .AnyAsync(v => v.Id != id && v.Name.ToLower() == dto.Name.ToLower());

            if (isDuplicate)
            {
                return Conflict(ApiResponse<VillaDTO>.Conflict(
                    $"A villa with the name: '{dto.Name}' already exists"
                ));
            }

            _mapper.Map(dto, existingVilla);
            existingVilla.UpdateDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

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
    public async Task<ActionResult<ApiResponse<VillaDTO>>> DeleteVilla(int id)
    {
        try
        {
            var existingVilla = await _dbContext.Villa.FindAsync(id);

            if(existingVilla == null) {
                return NotFound(ApiResponse<VillaDTO>.NotFound($"Villa with ID={id} was not found"));
            }

            _dbContext.Villa.Remove(existingVilla);
            await _dbContext.SaveChangesAsync();

            var response = ApiResponse<VillaDTO>.NoContent("Villa deleted successfully");
            return Ok(response); 
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<VillaDTO>.Error(
                HttpStatusCode.InternalServerError, $"An error occurred while deleting the villa",
                ex.Message
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}

