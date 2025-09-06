using Foundation;
using UIKit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Media;
using AvSafeDelivery.Services;
using System.Threading.Tasks;
using System;

namespace AvSafeDelivery.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public static AppDelegate? Instance { get; private set; }
    private TaskCompletionSource<string?>? _authTcs;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Instance = this;

        // register iOS platform services
        try
        {
            ServiceLocator.Register<ICameraService>(new iOSCameraService());
            ServiceLocator.Register<IAuthService>(new iOSAuthService());
        }
        catch { }

        return base.FinishedLaunching(application, launchOptions);
    }

    public Task<string?> StartAuthAsync()
    {
        _authTcs = new TaskCompletionSource<string?>();
        return _authTcs.Task;
    }

    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        try
        {
            var uri = new Uri(url.AbsoluteString);
            var query = uri.Query;
            var qs = ParseQueryString(query);
            qs.TryGetValue("code", out var code);
            if (!string.IsNullOrEmpty(code))
            {
                _authTcs?.TrySetResult(code);
            }
            else
            {
                qs.TryGetValue("error", out var err);
                _authTcs?.TrySetResult(err ?? "");
            }
        }
        catch (Exception ex)
        {
            _authTcs?.TrySetException(ex);
        }

        return true;
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