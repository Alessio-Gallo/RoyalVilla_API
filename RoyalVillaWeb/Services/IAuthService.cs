using System;
using RoyalVilla.DTO;

namespace RoyalVillaWeb.Services;

public interface IAuthService
{
    Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO);
    Task<T?> RegisterAsync<T>(RegistrationRequestDTO registrationRequestDTO);
}
