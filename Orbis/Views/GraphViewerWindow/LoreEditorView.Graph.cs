using Avalonia.Controls;
using Avalonia.Interactivity;
using Orb.Engine.Graph;
using Orbpad.Orbis.ViewModels;

namespace Orbpad.Orbis.Views;

public partial class LoreEditorView
{
    private void ViewGraph_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not LoreEditorViewModel viewModel)
        {
            return;
        }

        var owner =
            TopLevel.GetTopLevel(this) as Window;

        var graphWindow =
            new GraphViewerWindow(
                viewModel.Graph,
                viewModel.Title);

        if (owner is not null)
        {
            graphWindow.Show(owner);
        }
        else
        {
            graphWindow.Show();
        }
    }
}
