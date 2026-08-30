using System;

namespace Orbpad.Models;

public class Document
{
    public Guid Id { get; set; } =
        Guid.NewGuid();

    public string Text { get; set; } =
        string.Empty;

    public string? FilePath { get; set; }

    public string SavedText { get; set; } =
        string.Empty;

    public bool IsModified =>
        !string.Equals(
            Text,
            SavedText,
            StringComparison.Ordinal);

    public string? DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(
                    FilePath))
            {
                return System.IO.Path.GetFileName(
                    FilePath);
            }

            return null;
        }
    }

    public void MarkAsSaved()
    {
        SavedText =
            Text;
    }

    public void ResetToSaved()
    {
        Text =
            SavedText;
    }
}