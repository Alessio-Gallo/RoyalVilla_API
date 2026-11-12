
using System.Net.Http.Headers;
using System.Text.Json;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : IBaseService
{
    public IHttpClientFactory HttpClientFactory { get; set; } = httpClientFactory;
    private readonly ITokenProvider _tokenProvider = tokenProvider; 

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public ApiResponse<object> ResponseModel { get; set; } = new();

    public async Task<T?> SendAsync<T>(ApiRequest apiRequest, bool withBearer = true)
    {
        try
        {
            var client = HttpClientFactory.CreateClient("RoyalVillaAPI");
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(apiRequest.Url, uriKind: UriKind.Relative),
                Method = GetHttpMethod(apiRequest.ApiType),
            };

            var token = _tokenProvider.GetToken();
            if (withBearer && !string.IsNullOrEmpty(token))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if(apiRequest.Data != null)
            {
                if(apiRequest.Data is MultipartFormDataContent multipartContent)
                {
                    message.Content = multipartContent;
                }
                else
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: jsonSerializerOptions);
                }
            }

            
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"AUTHORIZATION HEADER: {message.Headers.Authorization}");
            Console.WriteLine("--------------------------------");

            var apiResponse = await client.SendAsync(message);

            var responseContent = await apiResponse.Content.ReadAsStringAsync();

            Console.WriteLine("--------------------------------");
            Console.WriteLine($"REQUEST: {message.Method} {message.RequestUri}");
            Console.WriteLine($"TOKEN PRESENT: {!string.IsNullOrEmpty(token)}");
            Console.WriteLine($"HTTP STATUS: {(int)apiResponse.StatusCode}");
            Console.WriteLine($"REASON: {apiResponse.ReasonPhrase}");
            Console.WriteLine("--------------------------------");

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                Console.WriteLine("API returned an empty response.");
                return default;
            }

            if (!apiResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("API request failed.");
            }

            return JsonSerializer.Deserialize<T>(
                responseContent,
                jsonSerializerOptions
            );
            // return await apiResponse.Content.ReadFromJsonAsync<T>(jsonSerializerOptions);
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
