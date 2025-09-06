using System;
using System.Threading.Tasks;
using AvSafeDelivery.Services;
using UIKit;
using Foundation;
using Photos;
using System.IO;

namespace AvSafeDelivery.iOS;

public class iOSCameraService : ICameraService
{
    private TaskCompletionSource<string?>? _tcs;

    public Task<string?> CapturePhotoAsync()
    {
        _tcs = new TaskCompletionSource<string?>();

        try
        {
            var picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            picker.AllowsEditing = false;

            picker.FinishedPickingMedia += OnFinishedPicking;
            picker.Canceled += OnCanceled;

            var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (vc.PresentedViewController != null)
                vc = vc.PresentedViewController;

            vc.PresentViewController(picker, true, null);
        }
        catch (Exception ex)
        {
            _tcs?.TrySetException(ex);
        }

        return _tcs!.Task;
    }

    private async void OnFinishedPicking(object sender, UIImagePickerMediaPickedEventArgs e)
    {
        try
        {
            var image = e.Info[UIImagePickerController.OriginalImage] as UIImage;
            if (image != null)
            {
                // save to temporary file
                var data = image.AsJPEG(0.9f);
                var filename = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                var cachePath = Path.GetTempPath();
                var filePath = Path.Combine(cachePath, filename);
                NSError err = null;
                data.AsStream().CopyTo(File.OpenWrite(filePath));

                _tcs?.TrySetResult(filePath);
            }
            else
            {
                _tcs?.TrySetResult(null);
            }
        }
        catch (Exception ex)
        {
            _tcs?.TrySetException(ex);
        }
        finally
        {
            if (sender is UIImagePickerController picker)
            {
                picker.FinishedPickingMedia -= OnFinishedPicking;
                picker.Canceled -= OnCanceled;
                picker.DismissViewController(true, null);
            }
        }
    }

    private void OnCanceled(object sender, EventArgs e)
    {
        try
        {
            _tcs?.TrySetResult(null);
        }
        finally
        {
            if (sender is UIImagePickerController picker)
            {
                picker.FinishedPickingMedia -= OnFinishedPicking;
                picker.Canceled -= OnCanceled;
                picker.DismissViewController(true, null);
            }
        }
    }
}

