using System;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class AuthService(
    IHttpClientFactory httpClientFactory, 
    ITokenProvider tokenProvider,
    IHttpContextAccessor httpContextAccessor) : BaseService(httpClientFactory, tokenProvider, httpContextAccessor), IAuthService
{
    private const string APIEndpoint = "/api/auth";
    
    public Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = loginRequestDTO,
            Url = $"{APIEndpoint}/login",
        }, withBearer: false);
    }

    public Task<T?> RegisterAsync<T>(RegistrationRequestDTO registrationRequestDTO)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = registrationRequestDTO,
            Url = $"{APIEndpoint}/register",
        }, withBearer: false);
    }

    Task<T?> IAuthService.RefreshTokenAsync<T>(RefreshTokenRequestDTO refreshTokenRequestDTO) where T : default
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = refreshTokenRequestDTO,
            Url = $"{APIEndpoint}/refresh-token",
        }, withBearer: false);
    }
}
