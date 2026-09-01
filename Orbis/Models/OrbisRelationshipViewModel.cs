using System;
using Orb.Engine.Graph;

namespace Orbpad.Orbis.Models;

/// <summary>
/// UI representation of an Orbis relationship.
/// </summary>
public sealed class OrbisRelationshipViewModel
{
    private readonly OrbRelationship _relationship;

    public OrbisRelationshipViewModel(OrbRelationship relationship)
    {
        _relationship = relationship
            ?? throw new ArgumentNullException(nameof(relationship));
    }

    public Guid Id => _relationship.Id;

    public string Type
    {
        get => _relationship.Type;
        set => _relationship.Type = value ?? string.Empty;
    }

    public Guid SourceId
    {
        get => _relationship.SourceId;
        set => _relationship.SourceId = value;
    }

    public Guid TargetId
    {
        get => _relationship.TargetId;
        set => _relationship.TargetId = value;
    }

    public OrbRelationship Relationship => _relationship;
}