using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Orbpad;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void DontSave_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}