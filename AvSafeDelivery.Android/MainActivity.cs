using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;
using System.IO;
using Android.Graphics;
using Android.Provider;
using AvSafeDelivery.Services;
using System;

namespace AvSafeDelivery.Android;

[IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataSchemes = new[] { "com.companyname.avsafedelivery" }, DataHosts = new[] { "callback" })]
[Activity(
    Label = "AvSafeDelivery.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private const int REQUEST_CAMERA = 1001;
    private TaskCompletionSource<string?>? _cameraTcs;

    private TaskCompletionSource<string?>? _authTcs;
    public static MainActivity? Instance { get; private set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Instance = this;

        // register platform services
        try
        {
            ServiceLocator.Register<ICameraService>(new AndroidCameraService());
            ServiceLocator.Register<IAuthService>(new AndroidAuthService());
        }
        catch (Exception ex)
        {
            // swallow registration errors but log if needed
            System.Diagnostics.Debug.WriteLine("Service registration failed: " + ex.Message);
        }
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    public Task<string?> CapturePhotoAsync()
    {
        _cameraTcs = new TaskCompletionSource<string?>();

        try
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            StartActivityForResult(intent, REQUEST_CAMERA);
        }
        catch (Exception ex)
        {
            _cameraTcs.TrySetException(ex);
        }

        return _cameraTcs.Task;
    }

    public Task<string?> StartAuthAsync()
    {
        _authTcs = new TaskCompletionSource<string?>();
        return _authTcs.Task;
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if (intent?.Data != null)
        {
            try
            {
                var uri = intent.Data;
                var code = uri.GetQueryParameter("code");
                if (!string.IsNullOrEmpty(code))
                {
                    _authTcs?.TrySetResult(code);
                }
                else
                {
                    var error = uri.GetQueryParameter("error");
                    _authTcs?.TrySetResult(error ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                _authTcs?.TrySetException(ex);
            }
        }
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != REQUEST_CAMERA) return;
        
        if (resultCode == Result.Ok && data is { Extras: not null })
        {
            try
            {
                var bmp = (Bitmap?)data.Extras.Get("data");
                if (bmp != null)
                {
                    var filename = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    var cachePath = CacheDir?.AbsolutePath ?? FilesDir?.AbsolutePath ?? ".";
                    var filePath = System.IO.Path.Combine(cachePath, filename);
                    using var fs = System.IO.File.OpenWrite(filePath);
                    bmp.Compress(Bitmap.CompressFormat.Jpeg!, 90, fs);
                    fs.Close();

                    _cameraTcs?.TrySetResult(filePath);
                    return;
                }
            }
            catch (Exception ex)
            {
                _cameraTcs?.TrySetException(ex);
                return;
            }
        }

        _cameraTcs?.TrySetResult(null);
    }
}