using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Orb.Engine.Graph;
using Orb.Engine.Serialization;
using Orbpad.Orbis.ViewModels;

namespace Orbpad.Orbis.Views;

/// <summary>
/// Visual editor for an Orbis .lore graph.
/// </summary>
public partial class LoreEditorView : UserControl
{
    public LoreEditorView()
    {
        InitializeComponent();
    }


    // ================================================================
    // SAVE
    // ================================================================

    private void Save_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is LoreEditorViewModel viewModel)
        {
            viewModel.RequestSave();
        }
    }


    // ================================================================
    // SAVE AS
    // ================================================================

    private void SaveAs_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is LoreEditorViewModel viewModel)
        {
            viewModel.RequestSaveAs();
        }
    }


    // ================================================================
    // CLOSE
    // ================================================================

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is LoreEditorViewModel viewModel)
        {
            viewModel.RequestClose();
        }
    }


    // ================================================================
    // ADD EXISTING ENTITY
    // ================================================================

    private async void AddExistingEntity_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not LoreEditorViewModel viewModel)
        {
            return;
        }

        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            return;
        }

        var files =
            await topLevel.StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Add Existing Orbis Entities",
                        AllowMultiple = true,
                        FileTypeFilter =
                        [
                            new FilePickerFileType("Orbis Entity")
                            {
                                Patterns = ["*.entity"]
                            }
                        ]
                    });

        foreach (var file in files)
        {
            string? path = file.TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            try
            {
                string json =
                    await File.ReadAllTextAsync(path);

                var entity =
                    EntitySerializer.Deserialize(json);

                viewModel.AddExistingEntity(entity);
            }
            catch (Exception ex)
            {
                // Keep the Lore Editor open if one selected file is invalid.
                await ShowErrorAsync(
                    topLevel,
                    $"Could not add '{Path.GetFileName(path)}'.",
                    ex.Message);
            }
        }
    }


    // ================================================================
    // RELATIONSHIP CONTEXT MENU
    // ================================================================

    private void AddRelationshipAndEdit_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not LoreEditorViewModel viewModel)
        {
            return;
        }

        var relationship = viewModel.AddRelationship();

        if (relationship is not null)
        {
            viewModel.SelectedRelationship = relationship;
            RelationshipEditorPanel.IsVisible = true;
        }
    }

    private void EditRelationship_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button
            || button.Tag is not LoreRelationshipViewModel relationship
            || DataContext is not LoreEditorViewModel viewModel)
        {
            return;
        }

        viewModel.SelectedRelationship = relationship;
        RelationshipEditorPanel.IsVisible = true;
    }

    private void CloseRelationshipEditor_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is LoreEditorViewModel viewModel)
        {
            viewModel.SelectedRelationship = null;
        }

        RelationshipEditorPanel.IsVisible = false;
    }

    private void RemoveRelationshipAndClose_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button
            || button.Tag is not LoreRelationshipViewModel relationship
            || DataContext is not LoreEditorViewModel viewModel)
        {
            return;
        }

        viewModel.RemoveRelationship(relationship);
        viewModel.SelectedRelationship = null;
        RelationshipEditorPanel.IsVisible = false;
    }

    private static async Task ShowErrorAsync(
        TopLevel owner,
        string title,
        string message)
    {
        var dialog =
            new Window
            {
                Title = title,
                Width = 460,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

        var panel =
            new StackPanel
            {
                Margin = new Avalonia.Thickness(24),
                Spacing = 14
            };

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18
        });

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        var close = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Padding = new Avalonia.Thickness(16, 8)
        };

        close.Click += (_, _) => dialog.Close();
        panel.Children.Add(close);
        dialog.Content = panel;

        if (owner is Window window)
        {
            await dialog.ShowDialog(window);
        }
        else
        {
            dialog.Show();
        }
    }


    // ================================================================
    // REMOVE ENTITY
    // ================================================================

    private void RemoveEntity_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not LoreEntityViewModel entity)
        {
            return;
        }

        if (DataContext is LoreEditorViewModel viewModel)
        {
            viewModel.RemoveEntity(entity);
        }
    }


}