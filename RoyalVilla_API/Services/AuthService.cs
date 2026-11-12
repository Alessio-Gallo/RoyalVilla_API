using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using Microsoft.AspNetCore.Identity;
using RoyalVilla_API.Services.IServices;
using System.IdentityModel.Tokens.Jwt;

namespace RoyalVilla_API.Services;

public class AuthService(
    ApplicationDbContext dbContext, IMapper mapper, 
    IConfiguration configuration, UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ITokenService tokenService
    ) : IAuthService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper; 
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ITokenService _tokenService = tokenService;


    public async Task<bool> IsEmailExistentAsync(string email)
    {
        return await _dbContext.ApplicationUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<TokenDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
    {
        try
        {
            var user = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(
                u => u.Email!.ToLower() == loginRequestDTO.Email.ToLower()
            );

            if(user == null) {
                return null;    // user not found 
            }

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (!isValid) {
                return null;    // Invalid password
            }

            //generate token
            var token = await _tokenService.GenerateJwtTokenAsync(user); 

            var tokenHandler = new JwtSecurityTokenHandler(); 
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jwtTokenId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;
            
            // generate new refresh token
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

            await _tokenService.SaveRefreshTokenAsync(user.Id, jwtTokenId, refreshToken, refreshTokenExpiry);

            TokenDTO tokenDTO = new() {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = jwtToken.ValidTo
            };

            return tokenDTO;
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred during user login", ex);
        }
    }

    public async Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (await IsEmailExistentAsync(registerationRequestDTO.Email))
                {
                    throw new InvalidOperationException($"User with email '{registerationRequestDTO.Email}' already exists");
                }

                ApplicationUser user = new()
                {
                    Email = registerationRequestDTO.Email,
                    Name = registerationRequestDTO.Name,
                    UserName= registerationRequestDTO.Email,
                    NormalizedEmail=registerationRequestDTO.Email.ToUpper(),
                    EmailConfirmed=true
                };

                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User registration failed: {errors}");
                }


                var role = string.IsNullOrEmpty(registerationRequestDTO.Role) ? "Customer" : registerationRequestDTO.Role;

                if(!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);
               

                var userDto = _mapper.Map<UserDTO>(user);
                userDto.Role = role;

                return userDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred during user registration", ex);
            }
        }

    public async Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
    {
        try
        {
            if (string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
            {
                return null; 
            }

            // Validate refresh token 
            var (isValid, userId, tokenFamilyId, tokenReused) = 
                await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

            // Token reuse detected
            if(tokenReused)
            {
                return null; 
            }

            // Token is invalid or expired
            if(!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenFamilyId))
            {
                return null; 
            }

            // Get user
            var user = await _dbContext.ApplicationUsers.FindAsync(userId);
            if(user == null)
            {
                return null; 
            }

            // Revoke old refresh token
            await _tokenService.RevokeRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

            // Generate new access & refresh tokens
            var token = await _tokenService.GenerateJwtTokenAsync(user); 

            var tokenHandler = new JwtSecurityTokenHandler(); 
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // New refresh token
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

            await _tokenService.SaveRefreshTokenAsync(user.Id, tokenFamilyId, refreshToken, refreshTokenExpiry);

            TokenDTO tokenDTO = new() {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = jwtToken.ValidTo
            };

            return tokenDTO;
        }
        catch (Exception ex)
        {       
            throw new InvalidOperationException("An unexpected error occurred during token refresh", ex);
        }
    }
}
