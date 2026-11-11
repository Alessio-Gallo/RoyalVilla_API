using RoyalVilla.DTO;
using RoyalVillaWeb.Extensions;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class VillaService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : 
    BaseService(httpClientFactory, tokenProvider), IVillaService
{
    private const string APIEndpoint = "/api/villa";

    public Task<T?> CreateAsync<T>(VillaCreateDTO dto)
    {
        var formData = dto.ToMultipartFormData();
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.POST,
            Data = formData, 
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
        var formData = dto.ToMultipartFormData();
        return SendAsync<T>(new ApiRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = formData, 
            Url = $"{APIEndpoint}/{dto.Id}"
        });
    }
}
