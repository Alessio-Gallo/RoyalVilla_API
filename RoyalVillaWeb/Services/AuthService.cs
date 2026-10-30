using System;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : 
    BaseService(httpClientFactory, httpContextAccessor), IAuthService
{
    private const string APIEndpoint = "/api/auth";
    
    public Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = loginRequestDTO,
            Url = $"{APIEndpoint}/login",
        });
    }

    public Task<T?> RegisterAsync<T>(RegistrationRequestDTO registrationRequestDTO)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = registrationRequestDTO,
            Url = $"{APIEndpoint}/register",
        });
    }
}
