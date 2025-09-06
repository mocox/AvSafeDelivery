using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Linq;
using AvSafeDelivery.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace AvSafeDelivery.Views;

public partial class PhotoView : UserControl
{
    public PhotoView()
    {
        InitializeComponent();

        // wire up named buttons declared in XAML
        var upload = this.FindControl<Button>("UploadButton");
        if (upload != null)
            upload.Click += UploadButton_Click;

        var take = this.FindControl<Button>("TakePhotoButton");
        if (take != null)
            take.Click += TakePhotoButton_Click;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        var window = this.VisualRoot as Window;
        if (window == null) return;

        try
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = "Select images"
            };

            var files = await window.StorageProvider.OpenFilePickerAsync(options);
            if (files?.Count > 0)
            {
                var paths = files.Select(f => f.Path?.LocalPath ?? f.Path?.ToString()).Where(p => p != null).ToArray()!;
                if (DataContext is PhotoViewModel vm)
                {
                    await vm.AddImagesFromPathsAsync(paths);
                }
            }
        }
        catch (System.Exception ex)
        {
            // set status on the VM if available
            if (DataContext is PhotoViewModel vm)
                vm.StatusMessage = "File picker failed: " + ex.Message;
        }
    }

    public void TakePhotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PhotoViewModel vm)
        {
            // Platform-specific camera capture must be implemented separately.
            // For now call the ViewModel command to set a status message.
            vm.TakePhotoCommand.Execute(null);
        }
    }
}
