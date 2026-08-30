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
        var document =
            new Document();

        _documents.Add(document);

        ActiveDocument =
            document;

        return document;
    }

    public void SetActiveDocument(
        Document document)
    {
        if (!ContainsDocument(document))
        {
            throw new InvalidOperationException(
                "The document is not managed by this DocumentManager.");
        }

        ActiveDocument =
            document;
    }

    public bool RemoveDocument(
        Document document)
    {
        int index =
            _documents.IndexOf(document);

        if (index < 0)
            return false;

        bool wasActive =
            ActiveDocument == document;

        _documents.RemoveAt(index);

        if (_documents.Count == 0)
        {
            ActiveDocument = null;
            return true;
        }

        if (wasActive)
        {
            int newIndex =
                Math.Min(
                    index,
                    _documents.Count - 1);

            ActiveDocument =
                _documents[newIndex];
        }

        return true;
    }

    public bool ContainsDocument(
        Document document)
    {
        foreach (var existingDocument
                 in _documents)
        {
            if (existingDocument == document)
                return true;
        }

        return false;
    }

    public Document? GetNextDocument()
    {
        if (_documents.Count == 0)
            return null;

        if (ActiveDocument is null)
            return _documents[0];

        int index =
            _documents.IndexOf(
                ActiveDocument);

        if (index < 0)
            return _documents[0];

        int nextIndex =
            (index + 1) %
            _documents.Count;

        return _documents[nextIndex];
    }

    public Document? GetPreviousDocument()
    {
        if (_documents.Count == 0)
            return null;

        if (ActiveDocument is null)
            return _documents[0];

        int index =
            _documents.IndexOf(
                ActiveDocument);

        if (index < 0)
            return _documents[0];

        int previousIndex =
            index - 1;

        if (previousIndex < 0)
        {
            previousIndex =
                _documents.Count - 1;
        }

        return _documents[previousIndex];
    }

    public Document? GetDocumentAt(
        int index)
    {
        if (index < 0 ||
            index >= _documents.Count)
        {
            return null;
        }

        return _documents[index];
    }
}