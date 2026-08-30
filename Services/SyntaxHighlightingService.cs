using System;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace Orbpad.Services;

public sealed class SyntaxHighlightingService : IDisposable
{
    private readonly TextEditor _editor;

    private readonly RegistryOptions _registryOptions;

    private readonly TextMate.Installation _textMateInstallation;

    private bool _disposed;

    public SyntaxHighlightingService(
        TextEditor editor,
        ThemeName theme = ThemeName.DarkPlus)
    {
        _editor = editor;

        _registryOptions =
            new RegistryOptions(theme);

        _textMateInstallation =
            _editor.InstallTextMate(
                _registryOptions);
    }

    // ============================================================
    // APPLY SYNTAX HIGHLIGHTING
    // ============================================================

    public void ApplyForFile(
        string? filePath)
    {
        if (_disposed)
            return;

        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            ClearSyntaxHighlighting();
            return;
        }

        string extension =
            System.IO.Path
                .GetExtension(filePath)
                .ToLowerInvariant();

        if (string.IsNullOrEmpty(extension))
        {
            ClearSyntaxHighlighting();
            return;
        }

        try
        {
            var language =
                _registryOptions
                    .GetLanguageByExtension(
                        extension);

            if (language is null)
            {
                ClearSyntaxHighlighting();
                return;
            }

            string scopeName =
                _registryOptions
                    .GetScopeByLanguageId(
                        language.Id);

            if (string.IsNullOrWhiteSpace(
                    scopeName))
            {
                ClearSyntaxHighlighting();
                return;
            }

            _textMateInstallation.SetGrammar(
                scopeName);
        }
        catch
        {
            ClearSyntaxHighlighting();
        }
    }

    // ============================================================
    // MANUAL LANGUAGE
    // ============================================================

    public bool ApplyLanguage(
        string languageId)
    {
        if (_disposed ||
            string.IsNullOrWhiteSpace(
                languageId))
        {
            return false;
        }

        try
        {
            string scopeName =
                _registryOptions
                    .GetScopeByLanguageId(
                        languageId);

            if (string.IsNullOrWhiteSpace(
                    scopeName))
            {
                return false;
            }

            _textMateInstallation.SetGrammar(
                scopeName);

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // CLEAR
    // ============================================================

    public void ClearSyntaxHighlighting()
    {
        if (_disposed)
            return;

        _textMateInstallation.SetGrammar(
            null);
    }

    // ============================================================
    // DISPOSAL
    // ============================================================

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _textMateInstallation.Dispose();
    }
}