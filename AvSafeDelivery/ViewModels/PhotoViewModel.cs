using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;
using AvSafeDelivery.Services;

namespace AvSafeDelivery.ViewModels;

public partial class PhotoViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Bitmap> _images = new ObservableCollection<Bitmap>();

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public PhotoViewModel()
    {
    }

    // Called from the view code-behind after picking files
    public async Task AddImagesFromPathsAsync(string[] paths)
    {
        foreach (var path in paths)
        {
            try
            {
                await using var stream = File.OpenRead(path);
                var bmp = await Task.Run(() => Bitmap.DecodeToHeight(stream, 800));
                Images.Add(bmp);
            }
            catch
            {
                StatusMessage = "Failed to load: " + path;
            }
        }
    }

    // simple stub for camera capture on mobile - platform-specific implementation required
    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var camera = ServiceLocator.Get<ICameraService>();
            var path = await camera.CapturePhotoAsync();
            if (!string.IsNullOrEmpty(path))
            {
                await AddImagesFromPathsAsync(new[] { path });
                StatusMessage = "Photo added.";
            }
            else
            {
                StatusMessage = "No photo captured.";
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = "Camera capture failed: " + ex.Message;
        }
    }

    [RelayCommand]
    private void RemoveImage(Bitmap image)
    {
        if (Images.Contains(image))
            Images.Remove(image);
    }
}
