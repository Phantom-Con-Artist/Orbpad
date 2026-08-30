using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Orbpad.Explorer;

public sealed class ExplorerItem : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _hasChildren;

    public ExplorerItem(
        string name,
        string fullPath,
        bool isDirectory)
    {
        Name =
            name;

        FullPath =
            fullPath;

        IsDirectory =
            isDirectory;
    }

    // ============================================================
    // IDENTITY
    // ============================================================

    public string Name { get; }

    public string FullPath { get; }

    public bool IsDirectory { get; }

    public bool IsPlaceholder { get; init; }

    // ============================================================
    // DISPLAY
    // ============================================================

    public string Icon
    {
        get
        {
            if (IsPlaceholder)
                return "…";

            return IsDirectory
                ? "📁"
                : "📄";
        }
    }

    // ============================================================
    // TREE STATE
    // ============================================================

    public bool IsExpanded
    {
        get =>
            _isExpanded;

        set
        {
            if (_isExpanded == value)
                return;

            _isExpanded =
                value;

            OnPropertyChanged();
        }
    }

    public bool HasChildren
    {
        get =>
            _hasChildren;

        internal set
        {
            if (_hasChildren == value)
                return;

            _hasChildren =
                value;

            OnPropertyChanged();
        }
    }

    public bool IsLoaded
    {
        get;
        internal set;
    }

    public ObservableCollection<ExplorerItem>
        Children
    {
        get;
    } =
        new();

    // ============================================================
    // NOTIFICATION
    // ============================================================

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