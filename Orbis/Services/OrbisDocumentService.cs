using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Orb.Engine.Graph;
using Orb.Engine.Serialization;

namespace Orbpad.Orbis.Services;

/// <summary>
/// Provides document-level operations for Orbis documents.
/// </summary>
public sealed class OrbisDocumentService
{
    private readonly OrbEngineService _engineService;

    /// <summary>
    /// Creates a new Orbis document service.
    /// </summary>
    public OrbisDocumentService(OrbEngineService engineService)
    {
        _engineService = engineService
            ?? throw new ArgumentNullException(nameof(engineService));
    }

    /// <summary>
    /// Determines whether a file is an Orbis document supported by Orbpad.
    /// </summary>
    public bool IsOrbisDocument(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        string extension = Path.GetExtension(filePath);

        return extension.Equals(".entity", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".lore", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bundle", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".world", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines the Orbis document type from a file path.
    /// </summary>
    public OrbisDocumentType GetDocumentType(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "A file path is required.",
                nameof(filePath));
        }

        string extension = Path.GetExtension(filePath);

        return extension.ToLowerInvariant() switch
        {
            ".entity" => OrbisDocumentType.Entity,
            ".lore" => OrbisDocumentType.Lore,
            ".bundle" => OrbisDocumentType.Bundle,
            ".world" => OrbisDocumentType.World,
            _ => OrbisDocumentType.Unknown
        };
    }

    /// <summary>
    /// Loads an Entity document from disk.
    /// </summary>
    public async Task<OrbEntity> LoadEntityAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateDocumentType(
            filePath,
            OrbisDocumentType.Entity);

        string json = await File.ReadAllTextAsync(
            filePath,
            cancellationToken);

        return EntitySerializer.Deserialize(json);
    }

    /// <summary>
    /// Saves an Entity document to disk.
    /// </summary>
    public async Task SaveEntityAsync(
        string filePath,
        OrbEntity entity,
        CancellationToken cancellationToken = default)
    {
        ValidateDocumentType(
            filePath,
            OrbisDocumentType.Entity);

        ArgumentNullException.ThrowIfNull(entity);

        string json = EntitySerializer.Serialize(entity);

        await File.WriteAllTextAsync(
            filePath,
            json,
            cancellationToken);
    }

    /// <summary>
    /// Loads a Lore document from disk.
    /// </summary>
    public async Task<OrbGraph> LoadLoreAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateDocumentType(
            filePath,
            OrbisDocumentType.Lore);

        string json = await File.ReadAllTextAsync(
            filePath,
            cancellationToken);

        return LoreSerializer.Deserialize(json);
    }

    /// <summary>
    /// Saves a Lore document to disk.
    /// </summary>
    public async Task SaveLoreAsync(
        string filePath,
        OrbGraph graph,
        CancellationToken cancellationToken = default)
    {
        ValidateDocumentType(
            filePath,
            OrbisDocumentType.Lore);

        ArgumentNullException.ThrowIfNull(graph);

        string json = LoreSerializer.Serialize(graph);

        await File.WriteAllTextAsync(
            filePath,
            json,
            cancellationToken);
    }

    /// <summary>
    /// Reads the raw contents of an Orbis document.
    /// </summary>
    public async Task<string> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateOrbisDocument(filePath);

        return await File.ReadAllTextAsync(
            filePath,
            cancellationToken);
    }

    /// <summary>
    /// Writes raw contents to an Orbis document.
    /// </summary>
    public async Task WriteAsync(
        string filePath,
        string content,
        CancellationToken cancellationToken = default)
    {
        ValidateOrbisDocument(filePath);

        await File.WriteAllTextAsync(
            filePath,
            content ?? string.Empty,
            cancellationToken);
    }

    private void ValidateOrbisDocument(string filePath)
    {
        if (!IsOrbisDocument(filePath))
        {
            throw new ArgumentException(
                $"The file is not a supported Orbis document: {filePath}",
                nameof(filePath));
        }
    }

    private void ValidateDocumentType(
        string filePath,
        OrbisDocumentType expectedType)
    {
        OrbisDocumentType actualType = GetDocumentType(filePath);

        if (actualType != expectedType)
        {
            throw new ArgumentException(
                $"Expected a {expectedType} document but received " +
                $"{actualType}: {filePath}",
                nameof(filePath));
        }
    }
}

/// <summary>
/// Identifies the supported Orbis document formats.
/// </summary>
public enum OrbisDocumentType
{
    /// <summary>
    /// Unknown or unsupported document type.
    /// </summary>
    Unknown,

    /// <summary>
    /// An Orbis entity document.
    /// </summary>
    Entity,

    /// <summary>
    /// An Orbis lore document.
    /// </summary>
    Lore,

    /// <summary>
    /// An Orbis bundle document.
    /// </summary>
    Bundle,

    /// <summary>
    /// An Orbis world document.
    /// </summary>
    World
}