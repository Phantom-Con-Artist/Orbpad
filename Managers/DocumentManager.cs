using System;
using System.Collections.Generic;
using Orbpad.Models;

namespace Orbpad.Managers;

public class DocumentManager
{
    private readonly List<Document> _documents = new();

    public IReadOnlyList<Document> Documents =>
        _documents;

    public Document? ActiveDocument { get; private set; }

    public Document CreateDocument()
    {
        var document = new Document();

        _documents.Add(document);

        ActiveDocument = document;

        return document;
    }

    public void SetActiveDocument(Document document)
    {
        if (!_documents.Contains(document))
        {
            throw new InvalidOperationException(
                "The document is not managed by this DocumentManager.");
        }

        ActiveDocument = document;
    }

    public bool RemoveDocument(Document document)
    {
        if (!_documents.Remove(document))
            return false;

        if (ActiveDocument == document)
        {
            if (_documents.Count > 0)
            {
                ActiveDocument = _documents[^1];
            }
            else
            {
                ActiveDocument = null;
            }
        }

        return true;
    }
}