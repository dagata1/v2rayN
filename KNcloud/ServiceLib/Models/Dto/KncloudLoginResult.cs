namespace ServiceLib.Models.Dto;

public class KncloudLoginResult
{
    public string Token { get; set; } = string.Empty;

    public int IsAdmin { get; set; }

    public string AuthData { get; set; } = string.Empty;

    public string SubscriptionUrl { get; set; } = string.Empty;
}
