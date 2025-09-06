using Avalonia.Controls;
using AvSafeDelivery.ViewModels;

namespace AvSafeDelivery.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}