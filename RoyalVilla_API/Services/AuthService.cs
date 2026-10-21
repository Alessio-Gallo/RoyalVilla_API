using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Services;

public class AuthService(ApplicationDbContext dbContext, IMapper mapper, IConfiguration configuration) : IAuthService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper; 
    private readonly IConfiguration _configuration = configuration;

    public async Task<bool> IsEmailExistentAsync(string email)
    {
        return await _dbContext.User.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
    {
        try
        {
            var user = await _dbContext.User.FirstOrDefaultAsync(
                u => u.Email.ToLower() == loginRequestDTO.Email.ToLower()
            );

            if(user == null || user.Password != loginRequestDTO.Password)
            {
                return null!; 
            }

            var token = GenerateJwtToken(user); 

            return new LoginResponseDTO 
            {
                UserDTO = _mapper.Map<UserDTO>(user),
                Token = token
            };      
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred during user login", ex);
        }
    }

    public async Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
    {
        try {
            if(await IsEmailExistentAsync(registrationRequestDTO.Email))
            {
                throw new InvalidOperationException($"User with email '{registrationRequestDTO.Email}' already exists");
            }

            User user = new()
            {
                Email = registrationRequestDTO.Email,
                Name = registrationRequestDTO.Name,
                Password = registrationRequestDTO.Password,
                Role = string.IsNullOrEmpty(registrationRequestDTO.Role) ? "Customer" : registrationRequestDTO.Role,
                CreatedDate = DateTime.UtcNow,
            };

            await _dbContext.User.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<UserDTO>(user);
        } 
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during user registration", ex);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
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
