using RoyalVilla.DTO;

namespace RoyalVilla_API.Services;

public interface IAuthService
{
    Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

    Task<TokenDTO> LoginAsync(LoginRequestDTO loginRequestDTO);

    Task<bool> IsEmailExistentAsync(string email);

    Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO);
}
