using System;

namespace AvSafeDelivery.Services;

public static class AuthConfig
{
    public static string? GetClientId(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "google" => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"),
            "facebook" => Environment.GetEnvironmentVariable("FACEBOOK_CLIENT_ID"),
            _ => null
        };
    }

    public static string GetRedirectUri(int port)
    {
        // Default to loopback redirect on localhost
        var env = Environment.GetEnvironmentVariable("OAUTH_REDIRECT_HOST");
        var host = string.IsNullOrWhiteSpace(env) ? "127.0.0.1" : env;
        return $"http://{host}:{port}/callback";
    }

    public static string GetMobileRedirectUri()
    {
        // Use a custom scheme that is registered in the native platform manifests.
        // Ensure AndroidManifest and iOS Info.plist register this scheme.
        return "com.companyname.avsafedelivery://callback";
    }
}
