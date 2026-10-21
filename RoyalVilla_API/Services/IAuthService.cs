using System;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Services;

public interface IAuthService
{
    Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);

    Task<bool> IsEmailExistentAsync(string email);
}
