using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class VillaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : 
    BaseService(httpClientFactory, httpContextAccessor), IVillaService
{
    private const string APIEndpoint = "/api/villa";

    public Task<T?> CreateAsync<T>(VillaCreateDTO dto, string token)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto, 
            Token = token, 
            Url = $"{APIEndpoint}"
        });
    }

    public Task<T?> DeleteAsync<T>(int id, string token)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.DELETE,
            Token = token, 
            Url = $"{APIEndpoint}/{id}"
        });
    }

    public async Task<T?> GetAllAsync<T>(string token)
    {
        return await SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.GET,
            Token = token, 
            Url = $"{APIEndpoint}"
        });
    }

    public Task<T?> GetAsync<T>(int id, string token)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.GET, 
            Token = token, 
            Url = $"{APIEndpoint}/{id}"
        });
    }

    public Task<T?> UpdateAsync<T>(VillaUpdateDTO dto, string token)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto, 
            Token = token, 
            Url = $"{APIEndpoint}/{dto.Id}"
        });
    }
}
