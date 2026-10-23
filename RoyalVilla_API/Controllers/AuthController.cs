using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Services;

namespace RoyalVilla_API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService, IMapper mapper) : ControllerBase
{
    private readonly IAuthService _authService = authService; 
    private readonly IMapper _mapper = mapper; 

    // Registration endpoint
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody] RegistrationRequestDTO registrationRequestDTO)
    {
        try 
        {    
            if(registrationRequestDTO == null)
            {
                return BadRequest(ApiResponse<UserDTO>.BadRequest("Registration data is required"));
            }

            if(await _authService.IsEmailExistentAsync(registrationRequestDTO.Email))
            {
                return Conflict(ApiResponse<UserDTO>.Conflict(
                    $"User with email: '{registrationRequestDTO.Email}' already exists"
                ));
            }

            var user = await _authService.RegisterAsync(registrationRequestDTO);

            if(user == null)
            {
                return BadRequest(ApiResponse<UserDTO>.BadRequest("Registration failed"));
            }

            // Auth service
            var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully"); 
            // return CreatedAtAction(nameof(Register), response);
            return StatusCode(StatusCodes.Status201Created, response);
        } 
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<UserDTO>.Error(
                HttpStatusCode.InternalServerError, "An error occurred during registration",ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }


    // Login endpoint
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
    {
        try
        {
            if(loginRequestDTO == null)
            {
                return BadRequest(ApiResponse<LoginResponseDTO>.BadRequest("Login data is required"));
            }

            var loginResponse = await _authService.LoginAsync(loginRequestDTO);

            if(loginResponse == null)
            {
                return BadRequest(ApiResponse<LoginResponseDTO>.BadRequest("Login failed"));
            }

            // Login OK 
            var response = ApiResponse<LoginResponseDTO>.Ok(loginResponse, "Login successful");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponse<LoginResponseDTO>.Error(
                HttpStatusCode.InternalServerError, "An error occurred during login", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}

