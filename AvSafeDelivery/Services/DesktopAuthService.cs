using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace AvSafeDelivery.Services;

public class DesktopAuthService : IAuthService
{
    public async Task<string> StartOAuthAsync(string provider)
    {
        var clientId = AuthConfig.GetClientId(provider);
        if (string.IsNullOrWhiteSpace(clientId))
            return "error: missing client id for " + provider;

        // Start HttpListener on a random loopback port
        var listener = new HttpListener();
        var port = GetRandomUnusedPort();
        var redirectUri = AuthConfig.GetRedirectUri(port);
        var prefix = $"http://127.0.0.1:{port}/";
        listener.Prefixes.Add(prefix);

        try
        {
            listener.Start();
        }
        catch (Exception ex)
        {
            return "error: cannot start local listener: " + ex.Message;
        }

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
            listener.Stop();
            return "error: unknown provider";
        }

        try
        {
            // open system browser
            OpenBrowser(authUrl);
        }
        catch (Exception ex)
        {
            listener.Stop();
            return "error: could not open browser: " + ex.Message;
        }

        try
        {
            // Wait for incoming connection with the redirect
            var context = await listener.GetContextAsync().ConfigureAwait(false);
            var req = context.Request;
            var query = req.Url.Query ?? string.Empty;
            var qs = ParseQueryString(query);
            qs.TryGetValue("code", out var code);

            // respond with a basic page
            var responseString = "<html><body>Please return to the app. You can close this window.</body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            listener.Stop();

            if (string.IsNullOrEmpty(code))
            {
                qs.TryGetValue("error", out var err);
                return "error: no code received: " + (err ?? "no_code");
            }

            return code;
        }
        catch (Exception ex)
        {
            try { listener.Stop(); } catch { }
            return "error: listening failed: " + ex.Message;
        }
    }

    private static int GetRandomUnusedPort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            // cross-platform way
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch
        {
            // fallback
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            else if (OperatingSystem.IsLinux())
                Process.Start("xdg-open", url);
            else if (OperatingSystem.IsMacOS())
                Process.Start("open", url);
            else
                throw;
        }
    }

    private static System.Collections.Generic.Dictionary<string, string> ParseQueryString(string query)
    {
        var dict = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(query)) return dict;
        var qs = query;
        if (qs.StartsWith("?")) qs = qs.Substring(1);
        var parts = qs.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            var kv = p.Split('=', 2);
            var key = Uri.UnescapeDataString(kv[0]);
            var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;
            dict[key] = val;
        }
        return dict;
    }
}
