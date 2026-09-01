using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Orb.Engine.Graph;
using Orb.Engine.Types;
using Orbpad.Orbis.Models;
using Orbpad.Orbis.Services;

namespace Orbpad.Orbis.ViewModels;

/// <summary>
/// ViewModel for editing an Orbis entity within an Orbis graph.
/// </summary>
public sealed class EntityEditorViewModel : INotifyPropertyChanged
{
    private readonly OrbEntity _entity;
    private readonly OrbGraph _graph;
    private readonly OrbisDocumentService? _documentService;

    private string _name = string.Empty;
    private string _type = string.Empty;

    /// <summary>
    /// Creates an editor for a new standalone entity.
    /// </summary>
    public EntityEditorViewModel()
        : this(
            new OrbEntity(),
            new OrbGraph())
    {
    }

    /// <summary>
    /// Creates an editor for an entity within a graph.
    /// </summary>
    /// <param name="entity">
    /// The entity being edited.
    /// </param>
    /// <param name="graph">
    /// The graph containing the entity and its relationships.
    /// </param>
    /// <param name="documentService">
    /// Optional document service used for persistence.
    /// </param>
    public EntityEditorViewModel(
        OrbEntity entity,
        OrbGraph graph,
        OrbisDocumentService? documentService = null)
    {
        _entity = entity
            ?? throw new ArgumentNullException(nameof(entity));

        _graph = graph
            ?? throw new ArgumentNullException(nameof(graph));

        _documentService = documentService;

        _name = entity.Name ?? string.Empty;
        _type = entity.Type ?? string.Empty;

        Properties = new ObservableCollection<OrbProperty>(
            entity.Properties.Values);

        Relationships =
            new ObservableCollection<OrbisRelationshipViewModel>();

        LoadRelationships();
    }

    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public Guid Id => _entity.Id;

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            value ??= string.Empty;

            if (_name == value)
                return;

            _name = value;
            _entity.Name = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    public string Type
    {
        get => _type;
        set
        {
            value ??= string.Empty;

            if (_type == value)
                return;

            _type = value;
            _entity.Type = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the properties belonging to the entity.
    /// </summary>
    public ObservableCollection<OrbProperty> Properties { get; }

    /// <summary>
    /// Gets the relationships connected to the entity.
    /// </summary>
    public ObservableCollection<OrbisRelationshipViewModel>
        Relationships { get; }

    /// <summary>
    /// Gets the underlying Orb Engine entity.
    /// </summary>
    public OrbEntity Entity => _entity;

    /// <summary>
    /// Gets the graph containing this entity.
    /// </summary>
    public OrbGraph Graph => _graph;

    /// <summary>
    /// Adds a property to the entity.
    /// </summary>
    /// <param name="name">
    /// The property name.
    /// </param>
    /// <param name="value">
    /// The property value.
    /// </param>
    public void AddProperty(
        string name,
        OrbValue value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Property name is required.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(value);

        var property = new OrbProperty
        {
            Name = name,
            Value = value
        };

        if (_entity.Properties.ContainsKey(name))
        {
            throw new InvalidOperationException(
                $"A property named '{name}' already exists.");
        }

        _entity.Properties[name] = property;

        Properties.Add(property);
    }

    /// <summary>
    /// Updates an existing property.
    /// </summary>
    public void UpdateProperty(
        string originalName,
        string name,
        OrbValue value)
    {
        if (string.IsNullOrWhiteSpace(originalName))
            throw new ArgumentException("Original property name is required.", nameof(originalName));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name is required.", nameof(name));

        ArgumentNullException.ThrowIfNull(value);

        if (!_entity.Properties.TryGetValue(originalName, out var property))
            throw new InvalidOperationException($"Property '{originalName}' does not exist.");

        if (!string.Equals(originalName, name, StringComparison.Ordinal) &&
            _entity.Properties.ContainsKey(name))
        {
            throw new InvalidOperationException(
                $"A property named '{name}' already exists.");
        }

        _entity.Properties.Remove(originalName);
        property.Name = name;
        property.Value = value;
        _entity.Properties[name] = property;

        int index = Properties.IndexOf(property);
        if (index >= 0)
            Properties[index] = property;
    }

    /// <summary>
    /// Removes a property from the entity.
    /// </summary>
    /// <param name="property">
    /// The property to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the property was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveProperty(
        OrbProperty property)
    {
        if (property is null)
            return false;

        bool removed =
            _entity.Properties.Remove(property.Name);

        if (removed)
        {
            Properties.Remove(property);
        }

        return removed;
    }

    /// <summary>
    /// Adds a relationship to the graph.
    /// </summary>
    /// <param name="type">
    /// The semantic relationship type.
    /// </param>
    /// <param name="targetId">
    /// The identifier of the target entity.
    /// </param>
    /// <returns>
    /// A ViewModel representing the newly created relationship.
    /// </returns>
    public OrbisRelationshipViewModel AddRelationship(
        string type,
        Guid targetId)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(
                "Relationship type is required.",
                nameof(type));
        }

        if (!_graph.ContainsEntity(targetId))
        {
            throw new InvalidOperationException(
                $"Target entity '{targetId}' does not exist in the graph.");
        }

        var relationship = new OrbRelationship
        {
            Type = type,
            SourceId = _entity.Id,
            TargetId = targetId
        };

        _graph.AddRelationship(relationship);

        var viewModel =
            new OrbisRelationshipViewModel(relationship);

        Relationships.Add(viewModel);

        return viewModel;
    }

    /// <summary>
    /// Adds an existing relationship to the editor.
    /// </summary>
    /// <param name="relationship">
    /// The relationship to add.
    /// </param>
    public void AddRelationship(
        OrbRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        if (!_graph.ContainsRelationship(relationship.Id))
        {
            _graph.AddRelationship(relationship);
        }

        if (Relationships.Any(
                relationshipViewModel =>
                    relationshipViewModel.Id == relationship.Id))
        {
            return;
        }

        Relationships.Add(
            new OrbisRelationshipViewModel(relationship));
    }

    /// <summary>
    /// Removes a relationship from the graph and editor.
    /// </summary>
    /// <param name="relationship">
    /// The relationship to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the relationship was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveRelationship(
        OrbisRelationshipViewModel relationship)
    {
        if (relationship is null)
            return false;

        bool removedFromGraph =
            _graph.RemoveRelationship(relationship.Id);

        bool removedFromCollection =
            Relationships.Remove(relationship);

        return removedFromGraph || removedFromCollection;
    }

    /// <summary>
    /// Refreshes the relationship collection from the graph.
    /// </summary>
    public void RefreshRelationships()
    {
        Relationships.Clear();

        LoadRelationships();
    }

    /// <summary>
    /// Gets the underlying entity for persistence.
    /// </summary>
    public OrbEntity GetEntity()
    {
        return _entity;
    }

    /// <summary>
    /// Gets the graph for persistence.
    /// </summary>
    public OrbGraph GetGraph()
    {
        return _graph;
    }

    /// <summary>
    /// Occurs when a ViewModel property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    private void LoadRelationships()
    {
        foreach (var relationship in
                 _graph.GetRelationships(_entity.Id))
        {
            Relationships.Add(
                new OrbisRelationshipViewModel(relationship));
        }
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}