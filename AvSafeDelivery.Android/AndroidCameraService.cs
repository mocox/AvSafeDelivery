using System.Threading.Tasks;
using AvSafeDelivery.Services;

namespace AvSafeDelivery.Android;

public class AndroidCameraService : ICameraService
{
    public async Task<string?> CapturePhotoAsync()
    {
        if (MainActivity.Instance == null)
            return null;

        return await MainActivity.Instance.CapturePhotoAsync();
    }
}

