using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using Microsoft.AspNetCore.Identity;

namespace RoyalVilla_API.Services;

public class AuthService(
    ApplicationDbContext dbContext, IMapper mapper, 
    IConfiguration configuration, UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager
    ) : IAuthService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper; 
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

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

            if(user == null) 
            {
                return null;    // user not found 
            }

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (!isValid) 
            {
                return null;    // Invalid password
            }

            //generate TOKEN
            var token = await GenerateJwtToken(user); 
            TokenDTO tokenDTO = new()
            {
                AccessToken = token
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

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]!);
        var roles = await _userManager.GetRolesAsync(user);

        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault()!),
            ]), 
            Expires = DateTime.UtcNow.AddDays(7), 
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor); 
    } 
}
