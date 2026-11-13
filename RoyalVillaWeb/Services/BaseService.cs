
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services;

public class BaseService(
    IHttpClientFactory httpClientFactory, 
    ITokenProvider tokenProvider,
    IHttpContextAccessor httpContextAccessor) : IBaseService
{
    public IHttpClientFactory HttpClientFactory { get; set; } = httpClientFactory;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // Session key for refresh lock
    private const string RefreshingTokenKey = "_RefreshingToken";

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private bool IsRefreshingToken
    {
        get => _httpContextAccessor.HttpContext?.Session.GetString(RefreshingTokenKey) == "true";
        set
        {
            if(value)
            {
                _httpContextAccessor.HttpContext?.Session.SetString(RefreshingTokenKey, "true");
            }
            else
            {
                _httpContextAccessor.HttpContext?.Session.Remove(RefreshingTokenKey);
            }
        }
    }

    public ApiResponse<object> ResponseModel { get; set; } = new();

    private async Task<bool> RefreshAccessTokenAsync()
    {
        try
        {
            if (IsRefreshingToken)
            {
                await Task.Delay(1000);
                var accessToken = _tokenProvider.GetAccessToken();
                if(accessToken != null)
                {
                    return true; 
                }
                return false; 
            }

            IsRefreshingToken = true; 

            var refreshToken = _tokenProvider.GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false; 
            }

            var client = HttpClientFactory.CreateClient("RoyalVillaAPI"); 
            var refreshRequest = new RefreshTokenRequestDTO
            {
                RefreshToken = refreshToken
            };

            var apiRequest = new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = refreshRequest,
                Url = "/api/auth/refresh-token"
            };

            var message = CreateRequestMessage(apiRequest, withBearer: false);
            var response = await client.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenDTO>>();

                if(result?.Success == true && result?.Data != null 
                && !string.IsNullOrEmpty(result.Data.AccessToken) 
                && !string.IsNullOrEmpty(result.Data.RefreshToken))
                {
                    // Update the tokens
                    _tokenProvider.SetToken(result.Data.AccessToken, result.Data.RefreshToken);
                    return true;
                }
            } 

            _tokenProvider.ClearToken();
            return false; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token refresh error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.Message}");
            _tokenProvider.ClearToken();
            return false; 
        }
        finally
        {
            IsRefreshingToken = false; 
        }
    }

    public async Task<T?> SendAsync<T>(ApiRequest apiRequest, bool withBearer = true)
    {
        try
        {
            var client = HttpClientFactory.CreateClient("RoyalVillaAPI");

            var message = CreateRequestMessage(apiRequest, withBearer);
            var apiResponse = await client.SendAsync(message);

            //401 -> we can try with refresh token
            if(apiResponse.StatusCode == HttpStatusCode.Unauthorized && withBearer && !IsRefreshingToken)
            {
                Console.WriteLine("Received Unauthorized - attempting token refresh");
                var refreshed = await RefreshAccessTokenAsync();

                if (refreshed)
                {
                    // Token refreshed successfully - retrying request
                    var retryMessage = CreateRequestMessage(apiRequest, withBearer);
                    apiResponse = await client.SendAsync(retryMessage);
                } 
                else
                {
                    _tokenProvider.ClearToken();
                    await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    _httpContextAccessor.HttpContext?.Response.Redirect("/auth/login");
                    return default; 
                }
            }

            var responseContent = await apiResponse.Content.ReadAsStringAsync();

            // Console.WriteLine("--------------------------------");
            // Console.WriteLine($"REQUEST: {message.Method} {message.RequestUri}");
            // Console.WriteLine($"TOKEN PRESENT: {!string.IsNullOrEmpty(apiRequest.Token)}");
            // Console.WriteLine($"HTTP STATUS: {(int)apiResponse.StatusCode}");
            // Console.WriteLine($"REASON: {apiResponse.ReasonPhrase}");
            // Console.WriteLine("--------------------------------");

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

    private HttpRequestMessage CreateRequestMessage(ApiRequest apiRequest, bool withBearer)
    {
        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(apiRequest.Url, uriKind: UriKind.Relative),
            Method = GetHttpMethod(apiRequest.ApiType),
        };

        var token = _tokenProvider.GetAccessToken();
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
        return message; 
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
