using CommunityToolkit.Mvvm.ComponentModel;

namespace AvSafeDelivery.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] 
    private string _greeting = "Welcome to Safe Delivery!";
    
    public MainViewModel(): base()
    {
    }
}