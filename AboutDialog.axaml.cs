using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Orbpad;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }
}
