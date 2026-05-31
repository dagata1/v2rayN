using System.Net.Http.Headers;
using System.Net.Mime;

namespace ServiceLib.Services;

public class KncloudAuthService
{
    private const string CloudDomainUrl = "https://aws.kncloud.top/api/domain/cloud";
    private const string DefaultDomain = "https://www.kncloud.top";
    private const string LoginPath = "/api/v1/passport/auth/login";
    private const string ForgetPasswordPath = "/api/v1/passport/auth/forget";
    private const string SubscribePathFormat = "/api/v1/client/subscribe?token={0}";

    private static string? _domain;

    public static async Task<string> GetForgetPasswordUrlAsync()
    {
        var domain = await GetDomainAsync();
        return BuildUrl(domain, ForgetPasswordPath);
    }

    public async Task<RetResult> LoginAsync(string email, string password)
    {
        if (email.IsNullOrEmpty() || password.IsNullOrEmpty())
        {
            return new RetResult(false, "请输入邮箱和密码");
        }

        try
        {
            using var client = new HttpClient();
            var domain = await GetDomainAsync(client);
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(domain, LoginPath));
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
                SubscriptionUrl = BuildUrl(domain, string.Format(SubscribePathFormat, token))
            };
            return new RetResult(true, "登录成功", result);
        }
        catch (Exception ex)
        {
            Logging.SaveLog("KNcloud login", ex);
            return new RetResult(false, $"登录失败：{ex.Message}");
        }
    }

    private static async Task<string> GetDomainAsync(HttpClient? client = null)
    {
        if (_domain.IsNotEmpty())
        {
            return _domain!;
        }

        var disposeClient = client is null;
        client ??= new HttpClient();
        try
        {
            using var response = await client.GetAsync(CloudDomainUrl);
            var responseText = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var root = JsonUtils.ParseJson(responseText);
                var domain = root?["data"]?["domain"]?.GetValue<string>()?.Trim().TrimEnd('/');
                if (domain.IsNotEmpty())
                {
                    _domain = domain;
                    return _domain;
                }
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog("KNcloud domain", ex);
        }
        finally
        {
            if (disposeClient)
            {
                client.Dispose();
            }
        }

        _domain = DefaultDomain;
        return _domain;
    }

    private static string BuildUrl(string domain, string path)
    {
        return $"{domain.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
