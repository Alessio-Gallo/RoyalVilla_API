
using System.Net.Http.Headers;
using System.Text.Json;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class BaseService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IBaseService
{
    public IHttpClientFactory HttpClientFactory { get; set; } = httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public ApiResponse<object> ResponseModel { get; set; } = new();

    public async Task<T?> SendAsync<T>(ApiRequest apiRequest)
    {
        try
        {
            var client = HttpClientFactory.CreateClient("RoyalVillaAPI");
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(apiRequest.Url, uriKind: UriKind.Relative),
                Method = GetHttpMethod(apiRequest.ApiType),
            };

            var token = _httpContextAccessor.HttpContext?.Session?.GetString(SD.SessionToken);
            if (!string.IsNullOrEmpty(token))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if(apiRequest.Data != null)
            {
                message.Content = JsonContent.Create(apiRequest.Data, options: jsonSerializerOptions);
            }

            var apiResponse = await client.SendAsync(message);

            return await apiResponse.Content.ReadFromJsonAsync<T>(jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}"); 
            return default!; 
        }
    }

    private static HttpMethod GetHttpMethod(SD.ApiType apiType)
    {
        return apiType switch
        {
            SD.ApiType.POST => HttpMethod.Post,
            SD.ApiType.PUT => HttpMethod.Put,
            SD.ApiType.DELETE => HttpMethod.Delete,
            _ => HttpMethod.Get
        };
    }
}
