<div align="center">

  <p>
    <img src="Assets/orb_v256.png"
         alt="Orbpad"
         width="128"
         height="128"/>
    &nbsp;&nbsp;&nbsp;&nbsp;
    <img src="docs/orbisprojectlogo.png"
         alt="Orbis Project"
         width="128"
         height="128"/>
  </p>

  <p>
    <strong>A project under</strong>
  </p>

  <h2>The Orbis Project</h2>

  <br/>

  <img src="docs/orbisprojectcover.png"
       alt="The Orbis Project"
       width="900"/>

  <br/><br/>

  <h1>Orbpad</h1>

  <p>
    <strong>
      A lightweight, fast, and focused desktop authoring environment for Windows.
    </strong>
    <br/>
    Built for plain text, source code, Markdown, and structured Orbis information.
  </p>

  <br/>

  <h3>An application within the Orbis Ecosystem</h3>

  <p>
    Orbpad is a human-facing authoring application within the Orbis Ecosystem,
    providing a focused desktop workspace for creating, editing, and managing
    information while working directly with Orbis entities, Lore documents,
    and relationships.
  </p>

  <br/>

</div>

<div align="center">

[![GitHub](https://img.shields.io/badge/GitHub-Orbpad-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Phantom-Con-Artist/Orbpad)
[![Orb.Engine](https://img.shields.io/badge/Orb.Engine-Orbis%20Core-0cc0df?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Phantom-Con-Artist/Orb)
[![Release](https://img.shields.io/badge/Release-v1.0.1-6f42c1?style=for-the-badge)](https://github.com/Phantom-Con-Artist/Orbpad/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/UI-Avalonia-9146FF?style=for-the-badge)](https://avaloniaui.net/)

</div>

---

## Overview

Orbpad is a Windows desktop editor built around two ideas:

1. Keep everyday editing fast and uncomplicated.
2. Give structured information a proper place alongside ordinary documents.

The editor handles plain text, source code, and Markdown with the usual tools you expect from a focused desktop editor: tabs, syntax highlighting, file navigation, search and replace, themes, drag-and-drop opening, recent files, and Windows file associations.

Orbpad v1.0.1 also introduces the first practical Orbis authoring workflow. You can create and manage **Entities** and **Lore** documents, establish relationships between entities, and inspect those relationships through a dedicated graph viewer.

Orbpad is distributed as a **self-contained Windows x64 application**, so the standard release does not require a separate .NET runtime installation.

---

## What is Orbis?

The **Orbis Project** is the larger ecosystem that Orbpad belongs to.

The central idea is to treat information as connected, structured objects rather than as a collection of unrelated files. Entities can be related to other entities, and Lore documents can represent a connected graph of those entities and their relationships.

Orbpad is the desktop authoring layer for that ecosystem.

The core engine is maintained separately:

**Orb.Engine repository:**  
https://github.com/Phantom-Con-Artist/Orb

Orb.Engine provides the underlying graph, entity, relationship, serialization, and structured-data functionality that Orbpad consumes.

---

## Features

### ✍️ Focused Editing

- Plain-text editing
- Source-code editing
- Multiple document tabs
- Undo / Redo
- Cut, Copy, Paste, Select All
- Word Wrap
- Line numbers
- Configurable editor font
- Configurable editor font size
- Recent files
- Drag-and-drop file opening

### 📝 Markdown

- Markdown source editing
- Live Markdown preview
- Split editing and preview
- Preview-only mode
- Markdown rendering powered by [Markdig](https://github.com/xoofx/markdig)
- Find and Replace support
- Windows `.md` file association

### 🎨 Syntax Highlighting

Orbpad uses [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) with TextMate-based language grammars to provide syntax highlighting for supported file types.

### 🗂️ Workspace File Explorer

The built-in File Explorer provides:

- Folder navigation
- File opening
- Resizable Explorer panel
- Explorer collapse/restore
- Opening documents directly from the workspace

### 📁 File Handling

- Open through the file picker
- Drag and drop
- Save
- Save As
- Recent files
- Windows "Open With" integration
- Image opening and viewing
- Native associations for supported Orbis file types

---

# Orbis Authoring

Orbpad v1.0.1 introduces the first integrated structured-information workflow powered by Orb.Engine.

## Entity Editor

An **Entity** represents a structured object in an Orbis graph.

The Entity Editor provides:

- Entity name
- Entity type
- Stable entity ID
- Custom properties
- Property creation
- Property editing
- Property removal
- Save and Save As workflows

The Entity Editor intentionally focuses on the entity itself.

Relationships are managed through the Lore Editor rather than duplicating relationship editing inside the Entity Editor.

---

## Lore Editor

A **Lore** document represents a connected Orbis graph.

The Lore Editor allows you to:

- Create a new Lore
- Open an existing `.lore` file
- Work with existing saved entities
- Add entities to the current Lore
- Establish relationships between entities
- Edit relationships
- View existing relationships
- Save Lore documents
- Save Lore documents under a new name
- Continue editing an existing Lore without losing its in-memory state

### Relationship Workflow

Relationships are first-class graph connections between existing entities.

The Lore Editor lets you select which entity is the source and which entity is the target, then establish a relationship between them.

This means a relationship such as:

```text
Avalon ── husband_of ──> Monami
```

is represented as a directed graph relationship rather than merely being text written inside an entity.

Bidirectional relationships are also supported:

```text
Avalon ── husband_of ──> Monami
Avalon <── wife_of ───── Monami
```

---

# Graph Viewer

The Lore Editor includes a dedicated **View Graph** action that opens the graph in a separate Graph Viewer window.

The Graph Viewer is intentionally independent from the main Orbpad workspace so that graph exploration does not congest the main editor.

## Graph Viewer Features

- Dedicated standalone graph window
- Entity nodes
- Directed relationships
- Relationship labels
- Directional arrowheads
- Bidirectional relationship visualization
- Separated relationship lanes
- Self-relationship rendering
- Zoom
- Pan
- Reset View
- Fit Graph
- Animated view transitions
- Entity hover feedback
- Entity Inspector
- Relationship Inspector

### Entity Inspector

Select an entity node to inspect the actual entity represented by that node.

The inspector exposes information such as:

- Name
- Type
- Entity ID
- Entity properties

### Relationship Inspector

Select a relationship label to inspect the actual relationship represented by that edge.

The inspector exposes:

- Relationship type
- Source entity
- Target entity
- Relationship ID
- Relationship properties

The graph viewer operates on the same `OrbGraph` used by the Lore Editor rather than maintaining a separate graph representation.

---

# Orbis File Formats

Orbpad v1.0.1 integrates two Orbis-specific document types.

## `.entity`

An `.entity` document represents an individual structured Orbis entity.

Conceptually:

```text
Entity
├── ID
├── Name
├── Type
└── Properties
```

Entities can be created and edited directly in Orbpad's Entity Editor.

## `.lore`

A `.lore` document represents an Orbis Lore graph.

Conceptually:

```text
Lore
├── Entities
│   ├── Entity A
│   ├── Entity B
│   └── Entity C
│
└── Relationships
    ├── A → B
    ├── B → C
    └── C → A
```

A Lore therefore contains the graph context that connects entities together.

Relationships created in the Lore Editor belong to the Lore graph rather than being written into the entity itself.

---

## Screenshots

### Main Editor

![Orbpad Editor](docs/screenshot-editor.png)

### File Explorer

![Orbpad File Explorer](docs/screenshot-explorer.png)

### Markdown Preview

![Orbpad Markdown Preview](docs/screenshot-markdown.png)

> Additional screenshots for the Entity Editor, Lore Editor, and Graph Viewer may be added as the UI documentation expands.

---

## Download

The latest Windows x64 release is available from the [GitHub Releases](https://github.com/Phantom-Con-Artist/Orbpad/releases) page.

### v1.0.1

| Package | Description |
|---|---|
| `Orbpad-1.0.1-win-x64-setup.exe` | Windows installer — recommended |
| Portable build | Self-contained Windows x64 application, if provided |

The installer uses Inno Setup and installs Orbpad with Windows Start Menu integration, optional desktop shortcut creation, an uninstaller, and file associations.

---

## Installation

### Windows Installer

1. Download `Orbpad-1.0.1-win-x64-setup.exe` from the [Releases](https://github.com/Phantom-Con-Artist/Orbpad/releases) page.
2. Run the installer.
3. Follow the setup wizard.
4. Choose whether to create a desktop shortcut.
5. Complete installation.
6. Launch Orbpad from the Start Menu or desktop shortcut.

The installer registers Orbpad with Windows and associates:

```text
.md
.entity
.lore
```

with Orbpad.

The installer also creates a Windows uninstaller entry.

### Portable Version

When a portable package is provided:

1. Extract it to a directory of your choice.
2. Run `Orbpad.exe`.

The published application is self-contained and does not require a separate .NET runtime installation.

---

## System Requirements

| Requirement | Details |
|---|---|
| Operating System | Windows 10 or Windows 11 |
| Architecture | x64 |
| Runtime | Self-contained; no separate .NET runtime required |
| Processor | 64-bit x64 processor |
| Memory | 4 GB minimum; 8 GB or more recommended |
| Storage | Approximately 200 MB minimum for a basic installation |
| Display | 1280 × 720 or higher recommended |

See [`REQUIREMENTS.md`](REQUIREMENTS.md) for the full requirements document.

---

## Markdown Support

Orbpad provides three Markdown modes:

| Mode | Description |
|---|---|
| **Edit** | Edit the Markdown source directly |
| **Split** | Edit Markdown and view rendered output side by side |
| **Preview** | View the rendered Markdown document |

Example:

```markdown
# Hello Orbpad

This is **bold text**.

This is *italic text*.
```

Markdown rendering is provided by Markdig.

---

## Find and Replace

Orbpad provides:

- Find
- Find Next
- Find Previous
- Replace
- Replace All
- Case-insensitive search

---

## Recent Files

Recently opened files are available through:

**File → Recent Files**

Entries that no longer point to usable files may be removed from the recent-file list.

---

## Windows File Associations

The v1.0.1 installer registers Orbpad with Windows for:

```text
.md
.entity
.lore
```

Examples:

```text
example.md
character.entity
kingdom.lore
```

These files can be opened through Windows using **Open With → Orbpad**.

---

## Themes

Orbpad includes five built-in themes:

| Theme | Description |
|---|---|
| **Orbpad Purple** | Dark violet interface with purple accents |
| **Orbpad Dark** | Neutral dark interface with charcoal surfaces |
| **Midnight** | Deep blue-dark interface with blue accents |
| **Forest** | Dark green interface with green accents |
| **Light** | Bright interface with a light background |

The default theme for a fresh Orbpad installation is **Orbpad Purple**.

---

## Keyboard Shortcuts

| Action | Shortcut |
|---|---|
| New document | `Ctrl + N` |
| Open file | `Ctrl + O` |
| Save | `Ctrl + S` |
| Save As | `Ctrl + Shift + S` |
| Undo | `Ctrl + Z` |
| Redo | `Ctrl + Y` |
| Cut | `Ctrl + X` |
| Copy | `Ctrl + C` |
| Paste | `Ctrl + V` |
| Select All | `Ctrl + A` |
| Close document | `Ctrl + W` |
| Next document | `Ctrl + Tab` |
| Previous document | `Ctrl + Shift + Tab` |
| Document 1–9 | `Ctrl + 1` … `Ctrl + 9` |

---

## Configuration

Orbpad stores application settings such as:

- Selected theme
- Toolbar visibility
- Status bar visibility
- Line number visibility
- Word Wrap state
- Editor font
- Editor font size
- Window size
- Window position
- Recent files

These settings are restored when Orbpad starts again.

---

## Technology

Orbpad v1.0.1 is built with:

- [.NET 10](https://dotnet.microsoft.com/)
- [Avalonia UI](https://avaloniaui.net/)
- [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
- [TextMateSharp](https://github.com/danipen/TextMateSharp)
- [Markdig](https://github.com/xoofx/markdig)
- [Avalonia.Controls.WebView](https://github.com/AvaloniaUI/Avalonia.Controls.WebView)
- **[Orb.Engine](https://github.com/Phantom-Con-Artist/Orb)**

Orb.Engine is the structured-data and graph engine used by Orbpad for Orbis entity, relationship, Lore, serialization, and graph functionality.

Orbpad is distributed as a self-contained Windows x64 application.

---

## Project Structure

```text
Orbpad/
├── Assets/
│   └── Orbpad.ico
│
├── Explorer/
├── Managers/
├── Markdown/
├── Models/
├── Services/
├── Styles/
│
├── Orbis/
│   ├── Views/
│   │   ├── EntityEditorView.*
│   │   ├── LoreEditorView.*
│   │   └── GraphViewerWindow/
│   │       ├── GraphViewerWindow.axaml
│   │       ├── GraphViewerWindow.axaml.cs
│   │       └── GraphViewerWindow.Inspector.cs
│   └── ViewModels/
│
├── Installer/
│   └── Orbpad.iss
│
├── README.md
├── REQUIREMENTS.md
├── MANUAL.md
├── THIRD-PARTY-NOTICES.md
├── CHANGELOG.md
├── LICENSE
│
├── Orbpad.csproj
├── App.axaml
├── App.axaml.cs
├── MainWindow.axaml
├── MainWindow.axaml.cs
└── Program.cs
```

Generated build output such as `bin/` and `obj/` is intentionally excluded from version control.

---

## Documentation

| Document | Description |
|---|---|
| [User Manual](MANUAL.md) | Detailed usage guide |
| [System Requirements](REQUIREMENTS.md) | Platform and hardware requirements |
| [Third-Party Notices](THIRD-PARTY-NOTICES.md) | Dependency and license information |
| [Changelog](CHANGELOG.md) | Release history |
| [License](LICENSE) | Orbpad license |

### Related Project

| Project | Description |
|---|---|
| [Orb.Engine](https://github.com/Phantom-Con-Artist/Orb) | Core Orbis structured-information and graph engine |

---

## Contributing

Issues, bug reports, feature requests, and pull requests are welcome.

For larger changes, opening an issue first is recommended so the design and scope can be discussed before implementation.

Typical workflow:

```bash
git clone https://github.com/Phantom-Con-Artist/Orbpad.git
cd Orbpad

git checkout -b feature/my-feature

dotnet build
```

Make your changes, test them, and open a pull request against the main branch.

---

## The Orbis Ecosystem

Orbpad is part of the broader **Orbis Ecosystem**.

The current ecosystem is centered around a structured-information model in which data can be represented as entities, connected through relationships, and organized into higher-level documents such as Lore.

### Orb.Engine

[Orb.Engine](https://github.com/Phantom-Con-Artist/Orb) is the core engine repository.

It provides the reusable structured-information and graph foundation used by Orbpad.

### Orbpad

Orbpad is the desktop authoring application.

In v1.0.1, Orbpad provides:

```text
                Orbpad
                  │
        ┌─────────┴─────────┐
        │                   │
   Entity Editor       Lore Editor
        │                   │
        │            ┌──────┴──────┐
        │            │             │
     Entity      Relationships   Graph
                                 Viewer
                                   │
                         ┌─────────┴─────────┐
                         │                   │
                    Entity Inspector   Relationship Inspector
```

### Planned Orbis Components

The ecosystem may expand over time with additional document formats, authoring tools, schemas, queries, timelines, and other structured-information capabilities.

Planned formats and components should not be considered part of the v1.0.1 release unless explicitly documented as implemented.

---

## Release Philosophy

Orbpad v1.0.1 focuses on a stable foundation rather than exposing every possible graph feature.

The release deliberately concentrates on:

- Editing
- Entities
- Lore
- Relationships
- Graph visualization
- Entity inspection
- Relationship inspection
- Reliable saving and loading
- Windows integration

More advanced graph analysis and traversal features may be added in future releases as the Orbis engine and application mature.

---

## License

Orbpad is distributed under the **MIT License**.

Copyright © 2026 Subhradeep Sarkar.

See [`LICENSE`](LICENSE) for the complete license text.

Third-party dependencies remain subject to their respective licenses. See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for dependency licensing information.

---

## Developer

**Subhradeep Sarkar**

Orbpad is independently developed as part of the Orbis Project.

---

<div align="center">

```text
Orbpad · v1.0.1
Windows x64 · Self-contained · MIT License
Part of the Orbis Ecosystem
```

</div>
