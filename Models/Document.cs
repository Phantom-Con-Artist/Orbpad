namespace Orbpad.Models;

public class Document
{
    public string Text { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    public bool IsModified { get; set; }
}