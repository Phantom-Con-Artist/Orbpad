using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Orb.Engine.Graph;

namespace Orbpad.Orbis.ViewModels;

/// <summary>
/// ViewModel for editing an Orbis lore graph.
/// </summary>
public sealed class LoreEditorViewModel : INotifyPropertyChanged
{
    private readonly OrbGraph _graph;

    private LoreRelationshipViewModel? _selectedRelationship;

    private string _title = "Untitled Lore";
    private string _subtitle = "Orbis Lore Graph";

    private bool _isDirty;


    // ================================================================
    // CONSTRUCTORS
    // ================================================================

    /// <summary>
    /// Creates an empty lore document.
    /// </summary>
    public LoreEditorViewModel()
        : this(new OrbGraph())
    {
    }


    /// <summary>
    /// Creates a lore editor around an existing graph.
    /// </summary>
    public LoreEditorViewModel(
        OrbGraph graph)
    {
        _graph =
            graph
            ?? throw new ArgumentNullException(nameof(graph));

        Entities =
            new ObservableCollection<LoreEntityViewModel>();

        Relationships =
            new ObservableCollection<LoreRelationshipViewModel>();

        Refresh();
    }


    // ================================================================
    // GRAPH
    // ================================================================

    /// <summary>
    /// Gets the underlying Orbis graph.
    /// </summary>
    public OrbGraph Graph =>
        _graph;


    // ================================================================
    // COLLECTIONS
    // ================================================================

    /// <summary>
    /// Gets the entities displayed by the editor.
    /// </summary>
    public ObservableCollection<LoreEntityViewModel> Entities
    {
        get;
    }


    /// <summary>
    /// Gets the relationships displayed by the editor.
    /// </summary>
    public ObservableCollection<LoreRelationshipViewModel>
        Relationships
    {
        get;
    }


    // ================================================================
    // SELECTION
    // ================================================================

    /// <summary>
    /// Gets or sets the currently selected relationship.
    /// </summary>
    public LoreRelationshipViewModel? SelectedRelationship
    {
        get => _selectedRelationship;

        set
        {
            if (ReferenceEquals(
                    _selectedRelationship,
                    value))
            {
                return;
            }

            _selectedRelationship = value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(HasSelectedRelationship));
        }
    }

    public bool HasSelectedRelationship =>
        SelectedRelationship is not null;


    // ================================================================
    // DOCUMENT STATE
    // ================================================================

    public string Title
    {
        get => _title;

        set
        {
            value ??= "Untitled Lore";

            if (_title == value)
            {
                return;
            }

            _title = value;

            OnPropertyChanged();
        }
    }


    public string Subtitle
    {
        get => _subtitle;

        set
        {
            value ??= "Orbis Lore Graph";

            if (_subtitle == value)
            {
                return;
            }

            _subtitle = value;

            OnPropertyChanged();
        }
    }


    public bool IsDirty
    {
        get => _isDirty;

        private set
        {
            if (_isDirty == value)
            {
                return;
            }

            _isDirty = value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(Title));
        }
    }


    // ================================================================
    // COUNTS
    // ================================================================

    public string EntityCountText =>
        $"{Entities.Count} "
        + (Entities.Count == 1
            ? "entity"
            : "entities");


    public string RelationshipCountText =>
        $"{Relationships.Count} "
        + (Relationships.Count == 1
            ? "relationship"
            : "relationships");


    // ================================================================
    // ADD EXISTING ENTITY
    // ================================================================

    public bool AddExistingEntity(OrbEntity entity)
    {
        if (entity is null)
        {
            return false;
        }

        if (_graph.Entities.ContainsKey(entity.Id))
        {
            return false;
        }

        _graph.AddEntity(entity);

        Entities.Add(
            new LoreEntityViewModel(
                entity,
                this));

        MarkDirty();
        NotifyCounts();
        RefreshRelationshipDisplay();

        return true;
    }


    // ================================================================
    // REMOVE ENTITY
    // ================================================================

    public bool RemoveEntity(
        LoreEntityViewModel entity)
    {
        if (entity is null)
        {
            return false;
        }


        Guid entityId =
            entity.Id;


        bool removed =
            _graph.RemoveEntity(
                entityId);


        if (!removed)
        {
            return false;
        }


        Entities.Remove(
            entity);


        // ------------------------------------------------------------
        // Removing an entity also removes relationships connected
        // to that entity at the graph level.
        // ------------------------------------------------------------

        var validRelationshipIds =
            _graph.Relationships.Keys
                .ToHashSet();


        for (int i =
                 Relationships.Count - 1;
             i >= 0;
             i--)
        {
            if (!validRelationshipIds.Contains(
                    Relationships[i].Id))
            {
                Relationships.RemoveAt(i);
            }
        }


        if (SelectedRelationship is not null
            && !validRelationshipIds.Contains(
                SelectedRelationship.Id))
        {
            SelectedRelationship =
                Relationships.FirstOrDefault();
        }


        MarkDirty();

        RefreshRelationshipDisplay();

        return true;
    }


    // ================================================================
    // ADD RELATIONSHIP
    // ================================================================

    public LoreRelationshipViewModel? AddRelationship()
    {
        if (Entities.Count < 2)
        {
            return null;
        }


        LoreEntityViewModel source =
            Entities[0];


        LoreEntityViewModel target =
            Entities.FirstOrDefault(
                entity =>
                    entity.Id != source.Id)
            ?? Entities[0];


        if (source.Id == target.Id)
        {
            return null;
        }


        var relationship =
            new OrbRelationship
            {
                Type = "related_to",
                SourceId = source.Id,
                TargetId = target.Id
            };


        _graph.AddRelationship(
            relationship);


        var viewModel =
            new LoreRelationshipViewModel(
                relationship,
                this);


        Relationships.Add(
            viewModel);


        SelectedRelationship =
            viewModel;


        MarkDirty();

        return viewModel;
    }


    // ================================================================
    // REMOVE RELATIONSHIP
    // ================================================================

    public bool RemoveRelationship(
        LoreRelationshipViewModel relationship)
    {
        if (relationship is null)
        {
            return false;
        }


        bool removed =
            _graph.RemoveRelationship(
                relationship.Id);


        if (!removed)
        {
            return false;
        }


        Relationships.Remove(
            relationship);


        if (SelectedRelationship == relationship)
        {
            SelectedRelationship =
                null;
        }


        MarkDirty();

        return true;
    }


    // ================================================================
    // REFRESH
    // ================================================================

    public void Refresh()
    {
        Entities.Clear();

        Relationships.Clear();


        foreach (OrbEntity entity
                 in _graph.Entities.Values)
        {
            Entities.Add(
                new LoreEntityViewModel(
                    entity,
                    this));
        }


        foreach (OrbRelationship relationship
                 in _graph.Relationships.Values)
        {
            Relationships.Add(
                new LoreRelationshipViewModel(
                    relationship,
                    this));
        }


        SelectedRelationship =
            null;


        NotifyCounts();

        RefreshRelationshipDisplay();
    }


    // ================================================================
    // RELATIONSHIP DISPLAY
    // ================================================================

    internal void RefreshRelationshipDisplay()
    {
        foreach (
            LoreRelationshipViewModel relationship
            in Relationships)
        {
            relationship.Refresh();
        }
    }


    // ================================================================
    // ENTITY NAME
    // ================================================================

    internal void EntityChanged(
        LoreEntityViewModel entity)
    {
        MarkDirty();

        RefreshRelationshipDisplay();
    }


    // ================================================================
    // RELATIONSHIP CHANGED
    // ================================================================

    internal void RelationshipChanged(
        LoreRelationshipViewModel relationship)
    {
        MarkDirty();

        relationship.Refresh();
    }


    // ================================================================
    // SAVE REQUEST
    // ================================================================

    public event EventHandler?
        SaveRequested;


    public event EventHandler?
        SaveAsRequested;


    public event EventHandler?
        CloseRequested;


    public void RequestSave()
    {
        SaveRequested?.Invoke(
            this,
            EventArgs.Empty);
    }


    public void RequestSaveAs()
    {
        SaveAsRequested?.Invoke(
            this,
            EventArgs.Empty);
    }


    public void RequestClose()
    {
        CloseRequested?.Invoke(
            this,
            EventArgs.Empty);
    }


    // ================================================================
    // DIRTY STATE
    // ================================================================

    public void MarkDirty()
    {
        IsDirty = true;
    }


    public void MarkAsSaved()
    {
        IsDirty = false;
    }


    // ================================================================
    // UNIQUE ENTITY NAME
    // ================================================================

    private string CreateUniqueEntityName()
    {
        const string baseName =
            "New Entity";


        if (!Entities.Any(
                entity =>
                    string.Equals(
                        entity.Name,
                        baseName,
                        StringComparison.OrdinalIgnoreCase)))
        {
            return baseName;
        }


        int index = 2;


        while (Entities.Any(
                   entity =>
                       string.Equals(
                           entity.Name,
                           $"{baseName} {index}",
                           StringComparison.OrdinalIgnoreCase)))
        {
            index++;
        }


        return $"{baseName} {index}";
    }


    // ================================================================
    // NOTIFICATIONS
    // ================================================================

    private void NotifyCounts()
    {
        OnPropertyChanged(
            nameof(EntityCountText));

        OnPropertyChanged(
            nameof(RelationshipCountText));
    }


    public event PropertyChangedEventHandler?
        PropertyChanged;


    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}


// ====================================================================
// LORE ENTITY VIEW MODEL
// ====================================================================

/// <summary>
/// UI representation of an entity inside a lore graph.
/// </summary>
public sealed class LoreEntityViewModel
    : INotifyPropertyChanged
{
    private readonly OrbEntity _entity;
    private readonly LoreEditorViewModel _owner;


    public LoreEntityViewModel(
        OrbEntity entity,
        LoreEditorViewModel owner)
    {
        _entity =
            entity
            ?? throw new ArgumentNullException(
                nameof(entity));

        _owner =
            owner
            ?? throw new ArgumentNullException(
                nameof(owner));
    }


    public Guid Id =>
        _entity.Id;


    public string Name
    {
        get =>
            _entity.Name ?? string.Empty;

        set
        {
            value ??= string.Empty;

            if (_entity.Name == value)
            {
                return;
            }

            _entity.Name =
                value;

            OnPropertyChanged();

            _owner.EntityChanged(
                this);
        }
    }


    public string Type
    {
        get =>
            _entity.Type ?? string.Empty;

        set
        {
            value ??= string.Empty;

            if (_entity.Type == value)
            {
                return;
            }

            _entity.Type =
                value;

            OnPropertyChanged();

            _owner.EntityChanged(
                this);
        }
    }


    public OrbEntity Entity =>
        _entity;


    public event PropertyChangedEventHandler?
        PropertyChanged;


    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}


// ====================================================================
// LORE RELATIONSHIP VIEW MODEL
// ====================================================================

/// <summary>
/// UI representation of a relationship inside a lore graph.
/// </summary>
public sealed class LoreRelationshipViewModel
    : INotifyPropertyChanged
{
    private readonly OrbRelationship _relationship;
    private readonly LoreEditorViewModel _owner;


    public LoreRelationshipViewModel(
        OrbRelationship relationship,
        LoreEditorViewModel owner)
    {
        _relationship =
            relationship
            ?? throw new ArgumentNullException(
                nameof(relationship));

        _owner =
            owner
            ?? throw new ArgumentNullException(
                nameof(owner));
    }


    public Guid Id =>
        _relationship.Id;


    public string Type
    {
        get =>
            _relationship.Type ?? string.Empty;

        set
        {
            value ??= string.Empty;

            if (_relationship.Type == value)
            {
                return;
            }

            _relationship.Type =
                value;

            OnPropertyChanged();

            _owner.RelationshipChanged(
                this);
        }
    }


    public Guid SourceId
    {
        get =>
            _relationship.SourceId;

        set
        {
            if (_relationship.SourceId == value)
            {
                return;
            }

            _relationship.SourceId =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(SourceName));

            OnPropertyChanged(
                nameof(SourceEntity));

            OnPropertyChanged(
                nameof(DisplayText));

            _owner.RelationshipChanged(
                this);
        }
    }


    public Guid TargetId
    {
        get =>
            _relationship.TargetId;

        set
        {
            if (_relationship.TargetId == value)
            {
                return;
            }

            _relationship.TargetId =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(TargetName));

            OnPropertyChanged(
                nameof(TargetEntity));

            OnPropertyChanged(
                nameof(DisplayText));

            _owner.RelationshipChanged(
                this);
        }
    }


    public string SourceName =>
        FindEntityName(
            SourceId);


    public string TargetName =>
        FindEntityName(
            TargetId);


    public LoreEntityViewModel? SourceEntity
    {
        get => FindEntity(SourceId);
        set
        {
            if (value is null || SourceId == value.Id)
            {
                return;
            }

            SourceId = value.Id;
        }
    }


    public LoreEntityViewModel? TargetEntity
    {
        get => FindEntity(TargetId);
        set
        {
            if (value is null || TargetId == value.Id)
            {
                return;
            }

            TargetId = value.Id;
        }
    }


    public string DisplayText =>
        $"{SourceName}  →  {TargetName}";


    public OrbRelationship Relationship =>
        _relationship;


    public void Refresh()
    {
        OnPropertyChanged(
            nameof(SourceName));

        OnPropertyChanged(
            nameof(TargetName));

        OnPropertyChanged(
            nameof(DisplayText));

        OnPropertyChanged(
            nameof(Type));
    }


    private LoreEntityViewModel? FindEntity(
        Guid id)
    {
        return _owner.Entities.FirstOrDefault(
            entity => entity.Id == id);
    }


    private string FindEntityName(
        Guid id)
    {
        LoreEntityViewModel? entity =
            FindEntity(id);

        if (entity is not null)
        {
            return string.IsNullOrWhiteSpace(entity.Name)
                ? id.ToString()
                : entity.Name;
        }

        return id.ToString();
    }


    public event PropertyChangedEventHandler?
        PropertyChanged;


    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}