using System;
using System.Threading.Tasks;
using AvSafeDelivery.Services;
using Foundation;
using UIKit;

namespace AvSafeDelivery.iOS;

public class iOSAuthService : IAuthService
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
            var scope = Uri.EscapeDataString("openid email profile");
            authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}&access_type=offline&prompt=consent";
        }
        else if (provider.Equals("facebook", StringComparison.OrdinalIgnoreCase))
        {
            var scope = Uri.EscapeDataString("email,public_profile");
            authUrl = $"https://www.facebook.com/v10.0/dialog/oauth?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
        }
        else
        {
            return "error: unknown provider";
        }

        try
        {
            var nsUrl = new NSUrl(authUrl);
            // open in system browser
            UIApplication.SharedApplication.OpenUrl(nsUrl);
        }
        catch (Exception ex)
        {
            return "error: could not open browser: " + ex.Message;
        }

        var app = AppDelegate.Instance;
        if (app == null)
            return "error: app delegate not available";

        try
        {
            var code = await app.StartAuthAsync();
            return code ?? "error: no code";
        }
        catch (Exception ex)
        {
            return "error: auth failed: " + ex.Message;
        }
    }
}

