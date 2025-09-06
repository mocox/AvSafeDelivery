using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvSafeDelivery.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private void Login()
    {
        StatusMessage = "Email/password login is not implemented yet.";
    }

    [RelayCommand]
    private async Task LoginWithGoogle(){
        StatusMessage = "Google login is not implemented in this sample.";
    }

    [RelayCommand]
    private void LoginWithFacebook()
    {
        StatusMessage = "Facebook login is not implemented in this sample.";
    }
}

