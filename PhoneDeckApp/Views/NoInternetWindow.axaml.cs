using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PhoneDeckApp.Views;

public partial class NoInternetWindow : Window
{
    public NoInternetWindow() => InitializeComponent();

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}