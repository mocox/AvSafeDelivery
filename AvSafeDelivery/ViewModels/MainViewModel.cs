using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AvSafeDelivery.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] 
    private string _greeting = "Welcome to Safe Delivery!";
    
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public MainViewModel(): base()
    {
        // Start on Login view
        CurrentViewModel = new LoginViewModel();
    }

    [RelayCommand]
    private void ShowLogin()
    {
        CurrentViewModel = new LoginViewModel();
    }

    [RelayCommand]
    private void ShowRegister()
    {
        CurrentViewModel = new RegisterViewModel();
    }

    [RelayCommand]
    private void ShowPhoto()
    {
        CurrentViewModel = new PhotoViewModel();
    }
}