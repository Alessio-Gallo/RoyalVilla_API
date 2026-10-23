using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers;

[Route("api/villa-amenities")]
[ApiController]
public class VillaAmenitiesController(ApplicationDbContext dbContext, IMapper mapper) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext; 
    private readonly IMapper _mapper = mapper;


    // Get all amenities
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmenitiesDTO>>>> GetAllVillaAmenities()
    {
        var villaAmenities = await _dbContext.VillaAmenities
            .AsNoTracking()         //readonly query
            .Include(a => a.Villa)  // performs a join
            .ToListAsync();

        var response = ApiResponse<IEnumerable<VillaAmenitiesDTO>>.Ok(
            _mapper.Map<List<VillaAmenitiesDTO>>(villaAmenities),
            "VillaAmenities retrieved successfully"
        );
        return Ok(response);
    }


    // Get amenities by Id
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetVillaAmenitiesById(int id)
    {
        try
        {
            if(id <= 0)
            {
                return BadRequest(
                    ApiResponse<VillaAmenitiesDTO>.BadRequest("VillaAmenities ID must be greater than 0")
                );
            }

            var amenity = await _dbContext.VillaAmenities
                .AsNoTracking()
                .Include(a => a.Villa)
                .FirstOrDefaultAsync(a => a.Id == id);

            if(amenity == null)
            {
                return NotFound(
                    ApiResponse<VillaAmenitiesDTO>.NotFound($"VillaAmenities with ID: '{id}' was not found")
                );
            }

            return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(
                _mapper.Map<VillaAmenitiesDTO>(amenity), "VillaAmenities retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                ApiResponse<VillaAmenitiesDTO>.Error(HttpStatusCode.InternalServerError,
                    $"An error occurred while retrieving VillaAmenities with ID {id}", ex.Message
                )
            );
        }
    }


    // Create villa amenities
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> CreateVillaAmenities(VillaAmenitiesCreateDTO dto)
    {
        try
        {
            var villaTarget = await _dbContext.Villa
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == dto.VillaId);

            if (villaTarget == null)
            {
                return Conflict(
                    ApiResponse<VillaAmenitiesDTO>.Conflict($"Villa with ID='{dto.VillaId}' does not exist")
                );
            }

            var name = dto.Name.Trim();     //remove leading and trailing spaces

            var duplicate = await _dbContext.VillaAmenities.AnyAsync(a =>
                a.VillaId == dto.VillaId && a.Name.ToLower() == name.ToLower());

            if (duplicate)
            {
                return Conflict(ApiResponse<VillaAmenitiesDTO>.Conflict(
                    $"Villa amenity '{name}' already exists for villa with ID: '{dto.VillaId}'"));
            }

            var villaAmenities = _mapper.Map<VillaAmenities>(dto);
            var now = DateTime.UtcNow;
            villaAmenities.CreatedDate = now;
            villaAmenities.UpdatedDate = now;

            await _dbContext.VillaAmenities.AddAsync(villaAmenities);
            await _dbContext.SaveChangesAsync();

            var responseData = _mapper.Map<VillaAmenitiesDTO>(villaAmenities);
            responseData.VillaName = villaTarget.Name;
            var response = ApiResponse<VillaAmenitiesDTO>.CreatedAt(
                responseData, "VillaAmenities created successfully"
            );

            return CreatedAtAction(nameof(GetVillaAmenitiesById), new { id = villaAmenities.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                ApiResponse<VillaAmenitiesDTO>.Error(HttpStatusCode.InternalServerError,
                    $"An error occurred while creating VillaAmenities for villa with ID: {dto.VillaId}", ex.Message
                )
            ); 
        }
    }

    
    // Update villa amenities
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<VillaAmenitiesDTO>.BadRequest(
                    "VillaAmenities ID in the URL does not match VillaAmenities ID in request body"));
            }

            var existingVillaAmenities = await _dbContext.VillaAmenities
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingVillaAmenities == null)
            {
                return NotFound(ApiResponse<VillaAmenitiesDTO>.NotFound($"VillaAmenities with ID='{id}' was not found"));
            }

            var villa = await _dbContext.Villa
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == dto.VillaId);

            if (villa == null)
            {
                return BadRequest(ApiResponse<VillaAmenitiesDTO>.BadRequest(
                    $"Villa with ID='{dto.VillaId}' does not exist"));
            }

            var name = dto.Name.Trim();     //remove leading and trailing spaces

            var duplicate = await _dbContext.VillaAmenities.AnyAsync(a =>
                a.Id != id && a.VillaId == dto.VillaId && a.Name.ToLower() == name.ToLower());

            if (duplicate)
            {
                return Conflict(ApiResponse<VillaAmenitiesDTO>.Conflict(
                    $"Amenity '{name}' already exists for villa with ID: '{dto.VillaId}'"));
            }

            _mapper.Map(dto, existingVillaAmenities);
            existingVillaAmenities.Name = name;
            existingVillaAmenities.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var result = _mapper.Map<VillaAmenitiesDTO>(existingVillaAmenities);
            result.VillaName = villa.Name;

            return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(result, "Villa amenity updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<VillaAmenitiesDTO>.Error(
                    HttpStatusCode.InternalServerError,
                    $"An error occurred while updating the VillaAmenities with ID={id}",
                    ex.Message));
        }
    }

    // Delete a VillaAmenities
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> DeleteVillaAmenities(int id)
    {
        try
        {
            var existingVillaAmenities = await _dbContext.VillaAmenities.FindAsync(id);

            if(existingVillaAmenities == null)
            {
                return NotFound(ApiResponse<VillaAmenitiesDTO>.NotFound($"VillaAmenities with ID='{id}' was not found"));
            }

            _dbContext.VillaAmenities.Remove(existingVillaAmenities);
            await _dbContext.SaveChangesAsync();

            var response = ApiResponse<VillaAmenitiesDTO>.NoContent($"Record deleted successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                ApiResponse<VillaAmenitiesDTO>.Error(HttpStatusCode.InternalServerError,
                    $"An error occurred while deleting VillaAmenities with ID={id}", ex.Message
                )
            ); 
        }
    } 
}