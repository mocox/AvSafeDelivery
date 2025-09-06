using System.Threading.Tasks;

namespace AvSafeDelivery.Services;

public interface ICameraService
{
    /// <summary>
    /// Capture a photo on the current platform and return a local file path to the saved image, or null if cancelled/failed.
    /// </summary>
    Task<string?> CapturePhotoAsync();
}

