using System.Net.Http.Headers;
using System.Net.Mime;

namespace ServiceLib.Services;

public class KncloudAuthService
{
    private const string LoginUrl = "https://www.kncloud.top/api/v1/passport/auth/login";
    public const string ForgetPasswordUrl = "https://www.kncloud.top/api/v1/passport/auth/forget";
    private const string SubscribeUrlFormat = "https://www.kncloud.top/api/v1/client/subscribe?token={0}";

    public async Task<RetResult> LoginAsync(string email, string password)
    {
        if (email.IsNullOrEmpty() || password.IsNullOrEmpty())
        {
            return new RetResult(false, "请输入邮箱和密码");
        }

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, LoginUrl);
            var json = JsonUtils.Serialize(new
            {
                email,
                password
            }, false);
            request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            using var response = await client.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new RetResult(false, $"登录失败：{(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var root = JsonUtils.ParseJson(responseText);
            var token = root?["data"]?["token"]?.GetValue<string>() ?? string.Empty;
            if (token.IsNullOrEmpty())
            {
                var message = root?["message"]?.GetValue<string>()
                    ?? root?["msg"]?.GetValue<string>()
                    ?? "登录失败，未返回订阅 token";
                return new RetResult(false, message);
            }

            var result = new KncloudLoginResult
            {
                Token = token,
                IsAdmin = root?["data"]?["is_admin"]?.GetValue<int>() ?? 0,
                AuthData = root?["data"]?["auth_data"]?.GetValue<string>() ?? string.Empty,
                SubscriptionUrl = string.Format(SubscribeUrlFormat, token)
            };
            return new RetResult(true, "登录成功", result);
        }
        catch (Exception ex)
        {
            Logging.SaveLog("KNcloud login", ex);
            return new RetResult(false, $"登录失败：{ex.Message}");
        }
    }
}
