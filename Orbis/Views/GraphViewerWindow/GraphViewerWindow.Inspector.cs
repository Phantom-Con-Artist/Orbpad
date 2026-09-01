using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Orb.Engine.Graph;
using Orb.Engine.Types;

namespace Orbpad.Orbis.Views;

public partial class GraphViewerWindow
{
    private Guid? _selectedEntityId;
    private Guid? _selectedRelationshipId;

    private void SelectEntity(Guid entityId)
    {
        if (!_graph.Entities.TryGetValue(entityId, out var entity))
        {
            ClearInspectorSelection();
            return;
        }

        _selectedEntityId = entityId;
        _selectedRelationshipId = null;

        EntityInspector.IsVisible = true;
        RelationshipInspector.IsVisible = false;
        InspectorEmptyState.IsVisible = false;

        EntityNameText.Text = string.IsNullOrWhiteSpace(entity.Name)
            ? "Unnamed Entity"
            : entity.Name;

        EntityTypeText.Text = string.IsNullOrWhiteSpace(entity.Type)
            ? "Entity"
            : entity.Type;

        EntityIdText.Text = entity.Id.ToString();

        BuildPropertyRows(
            EntityPropertiesPanel,
            entity.Properties);
    }

    private void SelectRelationship(Guid relationshipId)
    {
        if (!_graph.Relationships.TryGetValue(relationshipId, out var relationship))
        {
            ClearInspectorSelection();
            return;
        }

        _selectedEntityId = null;
        _selectedRelationshipId = relationshipId;

        EntityInspector.IsVisible = false;
        RelationshipInspector.IsVisible = true;
        InspectorEmptyState.IsVisible = false;

        RelationshipTypeText.Text = string.IsNullOrWhiteSpace(relationship.Type)
            ? "related_to"
            : relationship.Type;

        RelationshipIdText.Text = relationship.Id.ToString();
        RelationshipSourceText.Text = GetEntityDisplayName(relationship.SourceId);
        RelationshipTargetText.Text = GetEntityDisplayName(relationship.TargetId);

        BuildPropertyRows(
            RelationshipPropertiesPanel,
            relationship.Properties);
    }

    private string GetEntityDisplayName(Guid entityId)
    {
        if (!_graph.Entities.TryGetValue(entityId, out var entity))
        {
            return entityId.ToString();
        }

        var name = string.IsNullOrWhiteSpace(entity.Name)
            ? "Unnamed Entity"
            : entity.Name;

        return $"{name}\n{entityId}";
    }

    private void ClearInspectorSelection()
    {
        _selectedEntityId = null;
        _selectedRelationshipId = null;

        if (InspectorEmptyState is not null)
        {
            InspectorEmptyState.IsVisible = true;
        }

        if (EntityInspector is not null)
        {
            EntityInspector.IsVisible = false;
        }

        if (RelationshipInspector is not null)
        {
            RelationshipInspector.IsVisible = false;
        }
    }

    private static void BuildPropertyRows(
        StackPanel panel,
        IDictionary<string, OrbProperty> properties)
    {
        panel.Children.Clear();

        if (properties.Count == 0)
        {
            panel.Children.Add(
                new TextBlock
                {
                    Text = "No properties",
                    FontSize = 12,
                    Opacity = 0.45,
                    Margin = new Avalonia.Thickness(0, 4, 0, 0)
                });

            return;
        }

        foreach (var property in properties.Values)
        {
            var value = FormatOrbValue(property.Value);

            var row = new Border
            {
                Padding = new Avalonia.Thickness(10, 8),
                Margin = new Avalonia.Thickness(0, 0, 0, 6),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new SolidColorBrush(Color.Parse("#151C25")),
                BorderBrush = new SolidColorBrush(Color.Parse("#273241")),
                BorderThickness = new Avalonia.Thickness(1),
                Child = new StackPanel
                {
                    Spacing = 3,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = property.Name,
                            FontSize = 12,
                            FontWeight = Avalonia.Media.FontWeight.SemiBold,
                            Foreground = new SolidColorBrush(Color.Parse("#E7EDF5"))
                        },
                        new TextBlock
                        {
                            Text = property.Value.Type.ToString(),
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.Parse("#7C8CA3")),
                            LetterSpacing = 0.5
                        },
                        new TextBlock
                        {
                            Text = value,
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.Parse("#C7D2E0")),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            };

            panel.Children.Add(row);
        }
    }

    private static string FormatOrbValue(OrbValue value)
    {
        if (value.Type == OrbValueType.Null || value.Value is null)
        {
            return "null";
        }

        if (value.Value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        if (value.Value is Guid guid)
        {
            return guid.ToString();
        }

        if (value.Value is List<object?> list)
        {
            return list.Count == 0
                ? "[]"
                : $"[{string.Join(", ", list)}]";
        }

        if (value.Value is Dictionary<string, object?> dictionary)
        {
            return dictionary.Count == 0
                ? "{}"
                : $"{{{string.Join(", ", dictionary)}}}";
        }

        return value.Value.ToString() ?? string.Empty;
    }
}
