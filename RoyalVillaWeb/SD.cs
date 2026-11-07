
namespace RoyalVillaWeb;

public static class SD
{
    public enum ApiType
    {
        GET, POST, PUT, DELETE
    }

    public const string SessionToken = "JWTToken";
    public static string APIBaseUrl { get; set; } = string.Empty;

    
    public static string GetImageUrl(string imageUrl)
    {
        if(string.IsNullOrEmpty(imageUrl))
        {
            return "";
        }

        if (imageUrl.StartsWith("http"))
        {
            return imageUrl;
        }
        return $"{APIBaseUrl}{imageUrl}";
    }
}
