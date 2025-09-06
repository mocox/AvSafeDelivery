using System.Threading.Tasks;
using AvSafeDelivery.Services;
using Android.Content;
using Android.App;
using System;

namespace AvSafeDelivery.Android;

public class AndroidAuthService : IAuthService
{
    public async Task<string> StartOAuthAsync(string provider)
    {
        var clientId = AuthConfig.GetClientId(provider);
        if (string.IsNullOrWhiteSpace(clientId))
            return "error: missing client id for " + provider;

        var redirectUri = AuthConfig.GetMobileRedirectUri();
        string authUrl;
        if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
        {
            var scope = System.Uri.EscapeDataString("openid email profile");
            authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={System.Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}&access_type=offline&prompt=consent";
        }
        else if (provider.Equals("facebook", StringComparison.OrdinalIgnoreCase))
        {
            var scope = System.Uri.EscapeDataString("email,public_profile");
            authUrl = $"https://www.facebook.com/v10.0/dialog/oauth?client_id={clientId}&redirect_uri={System.Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
        }
        else
        {
            return "error: unknown provider";
        }

        // open browser
        try
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(authUrl));
            intent.AddFlags(ActivityFlags.NewTask);
            global::Android.App.Application.Context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            return "error: could not open browser: " + ex.Message;
        }

        // wait for MainActivity to provide the callback
        var main = AvSafeDelivery.Android.MainActivity.Instance;
        if (main == null)
            return "error: activity instance not available";

        try
        {
            var result = await main.StartAuthAsync();
            return result ?? "error: no code";
        }
        catch (Exception ex)
        {
            return "error: auth failed: " + ex.Message;
        }
    }
}
