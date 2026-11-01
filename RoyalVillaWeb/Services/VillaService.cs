using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class VillaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : 
    BaseService(httpClientFactory, httpContextAccessor), IVillaService
{
    private const string APIEndpoint = "/api/villa";

    public Task<T?> CreateAsync<T>(VillaCreateDTO dto)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto, 
            Url = $"{APIEndpoint}"
        });
    }

    public Task<T?> DeleteAsync<T>(int id)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.DELETE,
            Url = $"{APIEndpoint}/{id}"
        });
    }

    public async Task<T?> GetAllAsync<T>()
    {
        return await SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.GET,           
            Url = $"{APIEndpoint}"
        });
    }

    public Task<T?> GetAsync<T>(int id)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.GET, 
            Url = $"{APIEndpoint}/{id}"
        });
    }

    public Task<T?> UpdateAsync<T>(VillaUpdateDTO dto)
    {
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto, 
            Url = $"{APIEndpoint}/{dto.Id}"
        });
    }
}
