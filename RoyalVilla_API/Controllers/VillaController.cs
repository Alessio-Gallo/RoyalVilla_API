using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using Microsoft.AspNetCore.Authorization;

namespace RoyalVilla_API.Controllers;

[ApiController]
[Route("api/villa")]
//[Authorize]
// [Authorize(Roles = "Customer,Admin")]
public class VillaController(ApplicationDbContext dbContext, IMapper mapper) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper; 

    // Get all the records
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas([FromQuery] string? filterBy, 
        [FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] string? sortOrder = "asc")
    {
        var villasQuery = _dbContext.Villa.AsQueryable();
        if(!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
        {
            switch (filterBy.ToLower())
            {
                case "name":
                    villasQuery = villasQuery.Where(v => v.Name.ToLower().Contains(filterQuery.ToLower()));
                    break;
                case "details":
                    villasQuery = villasQuery.Where(v => v.Details.ToLower().Contains(filterQuery.ToLower()));
                    break;
                case "rate":
                    if(double.TryParse(filterQuery, out double rate)){
                        villasQuery = villasQuery.Where(v => v.Rate == rate);
                    }
                    break;
                case "minrate":
                    if(double.TryParse(filterQuery, out double minrate)){
                        villasQuery = villasQuery.Where(v => v.Rate >= minrate);
                    }
                    break;
                case "maxrate":
                    if(double.TryParse(filterQuery, out double maxrate)){
                        villasQuery = villasQuery.Where(v => v.Rate <= maxrate);
                    }
                    break;
                case "occupancy":
                    if(int.TryParse(filterQuery, out int occupancy)){
                        villasQuery = villasQuery.Where(v => v.Occupancy == occupancy);
                    }
                    break;
            }
        }

        // Sorting logic 
        if(!string.IsNullOrEmpty(sortBy))
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            villasQuery = sortBy.ToLower() switch
            {
                "name" => isDescending ? villasQuery.OrderByDescending(v => v.Name) 
                : villasQuery.OrderBy(v => v.Name), 
                "rate" => isDescending ? villasQuery.OrderByDescending(v => v.Rate) 
                : villasQuery.OrderBy(v => v.Rate), 
                "occupancy" => isDescending ? villasQuery.OrderByDescending(v => v.Occupancy) 
                : villasQuery.OrderBy(v => v.Occupancy), 
                "sqft" => isDescending ? villasQuery.OrderByDescending(v => v.Sqft) 
                : villasQuery.OrderBy(v => v.Sqft), 
                "id" => isDescending ? villasQuery.OrderByDescending(v => v.Id) 
                : villasQuery.OrderBy(v => v.Id), 
                _ => isDescending ? villasQuery.OrderByDescending(v => v.Id) 
                : villasQuery.OrderBy(v => v.Id),                  
            };
        }
        else
        {
           villasQuery = villasQuery.OrderBy(v => v.Id);
        }

        var villas = await villasQuery.ToListAsync();
        

        var responseData = _mapper.Map<List<VillaDTO>>(villas); 
        var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(responseData, "Villas retrieved successfully");

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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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

