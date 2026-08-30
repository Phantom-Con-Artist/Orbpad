using Avalonia.Controls;
using Avalonia.Interactivity;
using Orbpad.Models;

namespace Orbpad;

public partial class MainWindow : Window
{
    private Document _document;

    public MainWindow()
    {
        InitializeComponent();

        _document = new Document();
    }

    private void New_Click(object? sender, RoutedEventArgs e)
    {
        _document = new Document();

        Editor.Text = _document.Text;
    }
}