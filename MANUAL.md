# Orbpad User Manual

## Version 1.0.1

**Released:** September 2026  
**Developer:** Subhradeep Sarkar

---

# 1. Introduction

Orbpad is a lightweight Windows desktop editor designed for focused work with plain text, source code, Markdown, and structured information within the Orbis ecosystem.

Orbpad v1.0.1 introduces the first integrated Orbis authoring workflow. In addition to the normal text-editing experience, Orbpad can work with:

- `.entity` files
- `.lore` files
- Orbis entities
- Entity properties
- Directed relationships
- Lore graphs
- Graph visualization
- Entity inspection
- Relationship inspection

Orbpad is built on **Orb.Engine**, the core structured-information and graph engine used by the Orbis ecosystem.

**Orb.Engine repository:**

https://github.com/Phantom-Con-Artist/Orb

The application is distributed as a self-contained Windows x64 application.

---

# 2. Main Window

The main Orbpad interface contains the primary editor workspace and supporting controls.

The exact layout may vary slightly depending on the selected theme, window size, and enabled interface options.

The main application provides:

- Menu bar
- Toolbar
- Document tabs
- Text editor
- File Explorer
- Markdown preview
- Search and Replace
- Status bar
- Orbis Entity Editor
- Orbis Lore Editor

The File Explorer can be resized or hidden to provide more room for editing.

---

# 3. Creating a Document

To create a normal text document, select:

**File → New**

or press:

```text
Ctrl + N
```

Orbpad creates a new untitled document in a new tab.

The `+` control beside the document tabs can also be used to create a new document.

For structured Orbis content, use the dedicated Entity or Lore workflows described later in this manual.

---

# 4. Opening a File

To open a file, select:

**File → Open**

or press:

```text
Ctrl + O
```

Choose a file using the Windows file picker.

Files can also be opened through:

- File Explorer
- Drag and drop
- Recent Files
- Windows Open With
- Registered file associations

The v1.0.1 installer registers Orbpad for:

```text
.md
.entity
.lore
```

---

# 5. Saving a File

To save the current document:

**File → Save**

or:

```text
Ctrl + S
```

For a normal unsaved document, Orbpad opens the Save As workflow.

To choose a filename or location manually:

**File → Save As**

or:

```text
Ctrl + Shift + S
```

Structured Orbis documents use their dedicated serialization and save workflows.

---

# 6. Document Tabs

Orbpad supports multiple open documents.

Each document appears as a separate tab.

The active document is highlighted.

## Switch to the next document

```text
Ctrl + Tab
```

## Switch to the previous document

```text
Ctrl + Shift + Tab
```

## Switch directly to documents 1–9

```text
Ctrl + 1
Ctrl + 2
Ctrl + 3
...
Ctrl + 9
```

Tabs allow normal documents, Markdown documents, Entity documents, and Lore documents to coexist in the same Orbpad workspace.

---

# 7. Closing a Document

To close the active document:

```text
Ctrl + W
```

If the document contains unsaved changes, Orbpad asks whether the changes should be saved before closing.

Lore documents also preserve their own dirty state so that unsaved Lore changes can be protected when switching or closing the Lore editor.

---

# 8. Editing

Orbpad provides standard editing operations:

- Undo
- Redo
- Cut
- Copy
- Paste
- Select All

These operations are also available through the **Edit** menu.

---

# 9. Word Wrap

Word Wrap determines whether long lines wrap inside the editor.

Use:

**View → Word Wrap**

to enable or disable it.

---

# 10. Line Numbers

Use:

**View → Show Line Numbers**

to show or hide line numbers in the editor.

---

# 11. Fonts

Orbpad supports the following editor fonts:

- Inter
- Segoe UI
- Consolas
- Courier New

Open:

**View → Font**

to select a font.

---

# 12. Font Size

Open:

**View → Font Size**

Available sizes include:

```text
10
12
14
16
18
20
24
28
```

---

# 13. Markdown

Orbpad can be used as a Markdown editor.

Markdown source is edited directly in the main editor.

Example:

```markdown
# Hello Orbpad

This is **bold text**.

This is *italic text*.

- Item one
- Item two
- Item three
```

Markdown rendering is powered by **Markdig**.

---

# 14. Markdown Modes

Open:

**View → Markdown**

Orbpad provides three Markdown modes.

## Edit

Displays the Markdown source and allows direct editing.

## Split

Displays the Markdown editor and rendered preview together.

## Preview

Displays the rendered Markdown document.

---

# 15. Markdown Preview

Markdown preview converts Markdown source into a formatted view.

Orbpad uses Markdig for Markdown parsing and rendering.

Changes to Markdown source can be viewed through the split or preview modes.

---

# 16. Syntax Highlighting

Orbpad provides syntax highlighting for supported source files.

Highlighting is provided through AvaloniaEdit and TextMate-based grammars.

The appropriate grammar is selected according to the opened file when a supported language is recognized.

---

# 17. File Explorer

Orbpad includes a File Explorer on the left side of the application.

The Explorer provides a hierarchical view of files and folders inside the selected workspace.

The Explorer can be:

- Resized
- Collapsed
- Restored
- Used to open files directly

---

# 18. Opening a Workspace

Use the **Open Folder** control in the File Explorer.

After selecting a folder, its contents are displayed in the Explorer.

Files can then be opened directly from the workspace.

---

# 19. Resizing the Explorer

The File Explorer can be resized horizontally.

Position the pointer over the boundary between the Explorer and editor.

Drag:

```text
←  to make the Explorer narrower

→  to make the Explorer wider
```

The editor automatically adjusts to the available space.

---

# 20. Hiding the Explorer

Use the Explorer collapse button to hide the Explorer.

When hidden, the editor uses the available horizontal space.

Use the main **Explorer** toolbar control to restore it.

---

# 21. Drag and Drop

Files can be dragged from Windows File Explorer into Orbpad.

Drop a supported file onto the Orbpad window to open it.

This provides a quick alternative to the Open dialog.

---

# 22. Recent Files

Orbpad keeps a list of recently opened files.

Open:

**File → Recent Files**

to access them.

Files that no longer exist or cannot be accessed are removed from the usable recent-file list.

---

# 23. Find

Use the Find command from the toolbar or the appropriate menu command.

The search interface provides:

- Search
- Previous
- Next
- Close

Searches are case-insensitive.

---

# 24. Replace

The Find interface also provides:

- Replace
- Replace All

## Replace

Replaces the current matching occurrence.

## Replace All

Replaces all matching occurrences in the current document.

---

# 25. Images

Orbpad can open supported image files using:

**File → Open Image**

The image is displayed in Orbpad's image viewer.

---

# 26. Themes

Open:

**View → Themes**

Orbpad v1.0.1 provides:

- Orbpad Purple
- Orbpad Dark
- Midnight
- Forest
- Light

---

# 27. Orbpad Purple

**Orbpad Purple** is the default theme for a fresh Orbpad configuration.

It uses a dark violet interface with purple accent colors.

---

# 28. Orbpad Dark

**Orbpad Dark** provides a neutral dark interface with charcoal surfaces and violet accents.

---

# 29. Midnight

**Midnight** uses a deep blue-dark interface with blue accent colors.

---

# 30. Forest

**Forest** uses dark green surfaces and green accent colors.

---

# 31. Light

**Light** provides a bright interface with a light background and purple accent colors.

---

# 32. Windows Open With

Orbpad integrates with Windows file associations.

The v1.0.1 installer associates Orbpad with:

```text
.md
.entity
.lore
```

For example:

```text
example.md
character.entity
world.lore
```

You can open an associated file through Windows using:

**Right-click file → Open With → Orbpad**

Windows may also allow Orbpad to be selected as the default application for these file types.

---

# 33. Orbpad and the Orbis Ecosystem

Orbpad is a human-facing application within the **Orbis Project**.

The Orbis ecosystem is intended to represent information as structured objects connected through relationships rather than treating every piece of information as an isolated file.

Orbpad v1.0.1 provides the first integrated authoring workflow for this model.

The underlying engine is:

**Orb.Engine**

Repository:

https://github.com/Phantom-Con-Artist/Orb

Orb.Engine provides the graph and structured-information foundation used by Orbpad.

---

# 34. Entity Editor

The **Entity Editor** is used to create and edit an individual Orbis entity.

An entity can represent any structured object that you want to identify and manage inside the Orbis system.

Examples might include:

```text
Character
Location
Organization
Object
Event
Concept
```

The specific type is determined by the information stored in the entity.

---

# 35. Creating an Entity

Use the appropriate Entity creation action in Orbpad.

The Entity Editor opens with the new entity.

An entity contains:

- Name
- Type
- ID
- Properties

---

# 36. Entity Identity

The Entity Editor contains an **Identity** section.

## Name

The human-readable name of the entity.

Example:

```text
Avalon
```

## Type

The entity's type.

Example:

```text
Character
```

## ID

The entity's unique identifier.

The ID identifies the entity independently of its display name.

The ID should normally be treated as a stable identifier and should not be changed casually.

---

# 37. Entity Properties

Properties contain custom structured information belonging to the entity.

Examples:

```text
age = 42
title = King
occupation = ruler
```

Properties are managed directly in the Entity Editor.

The editor supports:

- Add Property
- Edit Property
- Remove Property

The exact property value representation depends on the Orb.Engine data model.

---

# 38. Saving an Entity

Use the normal save workflow to save an Entity.

The Entity Editor supports saving the entity to an `.entity` file.

The saved file contains the entity's structured identity and properties.

---

# 39. `.entity` Files

An `.entity` file represents an individual Orbis entity.

Conceptually:

```text
Entity
├── ID
├── Name
├── Type
└── Properties
```

Orbpad can open `.entity` files through:

- File → Open
- File Explorer
- Drag and drop
- Windows Open With

The v1.0.1 installer registers `.entity` files with Orbpad.

---

# 40. Entity Editor and Relationships

The Entity Editor deliberately focuses on the entity itself.

The **Relationships panel is not part of the v1.0.1 Entity Editor**.

Relationships are managed through the **Lore Editor**, where the graph context exists and multiple entities can be connected.

This avoids maintaining two separate relationship-editing workflows.

---

# 41. Lore Editor

The **Lore Editor** is the structured authoring environment for an Orbis Lore graph.

A Lore provides a graph context containing entities and relationships.

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

---

# 42. Creating a New Lore

Use the **New Lore** workflow from Orbpad.

A new Lore opens in the Lore Editor.

A newly created Lore is initially untitled until it is saved.

Once the Lore is saved, the editor uses the saved filename as the Lore's displayed title.

---

# 43. Opening a Lore

Use:

**File → Open**

and select a `.lore` file.

Orbpad loads the Lore graph and opens it in the Lore Editor.

The Lore is represented internally through an `OrbGraph`.

---

# 44. Existing Entities in Lore

The Lore Editor works with entities that already exist rather than forcing the user to recreate them every time a relationship is needed.

This allows an existing entity to be selected and included in the Lore graph.

The purpose is to preserve entity identity and avoid accidentally creating duplicates of the same object.

---

# 45. Adding Entities to Lore

Use the entity-selection workflow in the Lore Editor to add an existing entity to the current Lore.

The selected entity retains its existing identity.

This is important when the same entity participates in multiple relationships or Lore documents.

---

# 46. Relationships

A relationship connects two entities in the Lore graph.

A relationship has:

- Source entity
- Target entity
- Relationship type
- Relationship identity
- Optional relationship properties

A relationship is directed.

For example:

```text
Avalon ── husband_of ──> Monami
```

This means the source is Avalon and the target is Monami.

---

# 47. Creating a Relationship

In the Lore Editor:

1. Select the first entity.
2. Select the second entity.
3. Choose or enter the relationship type.
4. Establish the relationship.
5. Save the Lore when finished.

The relationship is then part of the Lore graph.

---

# 48. Bidirectional Relationships

Two directed relationships can connect the same pair of entities in opposite directions.

For example:

```text
Avalon ── husband_of ──> Monami
Avalon <── wife_of ───── Monami
```

These are two separate relationships.

The Graph Viewer displays them on separate visual lanes so their directions remain understandable.

---

# 49. Relationship Labels

Relationships are shown in the graph with their relationship type.

Examples:

```text
friend_of
owns
located_in
works_for
husband_of
```

The label is associated with the underlying relationship object.

Selecting the relationship label opens the Relationship Inspector in the Graph Viewer.

---

# 50. Editing a Relationship

The Lore Editor provides the relationship editing workflow.

A relationship can be selected and its information can be modified through the relationship editor.

After changing the relationship, save the Lore to persist the change.

---

# 51. Saving a Lore

Use:

**File → Save**

or:

```text
Ctrl + S
```

The Lore Editor saves the current graph rather than converting the Lore into ordinary text-editor content.

The graph remains the source of truth for the Lore document.

---

# 52. Save As for Lore

Use:

**File → Save As**

or:

```text
Ctrl + Shift + S
```

Save As creates a copy of the current Lore graph at the selected location.

The Lore Editor updates its current Lore path and title to the newly saved file.

The new file is still the same graph content unless you subsequently edit it.

---

# 53. Lore Titles

When a Lore file is opened, the editor uses the filename as the displayed Lore title.

For example:

```text
Kingdoms.lore
```

is displayed as:

```text
Kingdoms
```

A new unsaved Lore is displayed as:

```text
Untitled Lore
```

---

# 54. `.lore` Files

A `.lore` file represents a Lore graph.

Conceptually:

```text
Lore
├── Entity
├── Entity
├── Entity
│
└── Relationships
    ├── Relationship
    ├── Relationship
    └── Relationship
```

The Lore document provides the context in which entities are connected.

Relationships are therefore associated with the Lore graph rather than being treated as ordinary text inside an Entity Editor.

---

# 55. Lore Dirty State

The Lore Editor tracks whether the current graph has unsaved changes.

If the Lore is modified and then closed, Orbpad can ask whether the changes should be saved.

This protects changes made while switching between the Lore workspace and other documents.

---

# 56. Switching Between Editors

Orbpad can switch between normal documents and Orbis editors.

The Lore Editor and Entity Editor are separate application views.

The Lore Editor is responsible for:

- Lore graph editing
- Entity participation in the graph
- Relationship creation
- Relationship editing
- Graph access

The Entity Editor is responsible for:

- Entity identity
- Entity properties

---

# 57. Graph Viewer

The Lore Editor provides a **View Graph** action.

Selecting it opens the **Graph Viewer** in a separate window.

The Graph Viewer is intentionally separated from the main Orbpad editor so that graph visualization does not congest the main application workspace.

---

# 58. Graph Viewer Overview

The Graph Viewer visualizes the current Lore's `OrbGraph`.

It displays:

- Entity nodes
- Relationships
- Relationship labels
- Directional arrows
- Bidirectional relationships
- Self relationships
- Entity hover effects
- Zoom
- Pan
- Reset View
- Fit Graph
- Entity Inspector
- Relationship Inspector

The viewer uses the same graph supplied by the Lore Editor.

It does not create an unrelated copy of the Lore's graph for display.

---

# 59. Graph Nodes

Each entity is displayed as a graph node.

A node shows:

- Entity name
- Entity type

The entity's identifier is used internally to connect the node to the actual `OrbEntity`.

Hovering over a node provides visual feedback.

Clicking the node selects the entity for inspection.

---

# 60. Directed Relationships in the Graph

A relationship is displayed as a directed connection.

Arrowheads indicate the target direction.

For example:

```text
Avalon ───────────────▶ Monami
```

represents:

```text
Avalon → Monami
```

---

# 61. Bidirectional Graph Relationships

When two entities have relationships in both directions, the Graph Viewer separates them into different lanes.

Example:

```text
             husband_of
          ╭──────────────▶
         ╱
Avalon ●                         ● Monami
         ╲
          ◀──────────────╯
              wife_of
```

This prevents the two directions from visually collapsing into one connection.

Multiple relationships between the same entities are also separated according to their relationship lanes.

---

# 62. Self Relationships

A relationship where the source and target are the same entity is rendered as a self-connection around the entity node.

For example:

```text
Avalon
  ↺ knows_self
```

Self relationships remain selectable through their relationship label.

---

# 63. Panning the Graph

Click and drag the graph canvas to pan around the graph.

The graph view can be repositioned without changing the underlying graph.

---

# 64. Zooming the Graph

Use the mouse wheel over the Graph Viewer to zoom.

Zooming is centered around the pointer position so that the relevant area remains under the cursor.

---

# 65. Reset View

Use:

**Reset View**

to return the graph to the default zoom and pan position.

The transition is animated.

---

# 66. Fit Graph

Use:

**Fit Graph**

to rebuild and reset the displayed graph view.

This is useful after changing the graph or when you want to return to a known starting view.

---

# 67. Entity Inspector

The Graph Viewer includes a right-side **Entity Inspector**.

Click an entity node to inspect it.

The Entity Inspector displays information belonging to the actual selected entity.

It can show:

- Entity name
- Entity type
- Entity ID
- Entity properties

The inspector operates on the entity represented by the graph node rather than on a separate visualization-only object.

---

# 68. Relationship Inspector

The Graph Viewer also includes a **Relationship Inspector**.

Click a relationship label to inspect the corresponding relationship.

The Relationship Inspector can show:

- Relationship type
- Source entity
- Target entity
- Relationship ID
- Relationship properties

This allows a graph edge to be inspected without leaving the Graph Viewer.

---

# 69. Selecting an Entity

To select an entity:

1. Open the Graph Viewer.
2. Click an entity node.
3. The Inspector switches to the Entity view.
4. The selected entity's information is displayed.

Clicking an entity does not create a new entity.

---

# 70. Selecting a Relationship

To select a relationship:

1. Open the Graph Viewer.
2. Click the relationship label.
3. The Inspector switches to the Relationship view.
4. The selected relationship's information is displayed.

The relationship label is connected directly to its underlying relationship ID.

---

# 71. Clearing the Inspector

Clicking an empty area of the graph clears the current inspector selection.

The Inspector then returns to its neutral state.

This allows the graph to be viewed without keeping an object selected.

---

# 72. Graph Viewer and Lore Editor

The Graph Viewer is a visualization and inspection surface for the Lore Editor's graph.

The data flow is:

```text
Lore Editor
      │
      ▼
   OrbGraph
      │
      ▼
Graph Viewer
  ┌───┴────┐
  ▼        ▼
Entity   Relationship
Inspector Inspector
```

The Graph Viewer does not replace the Lore Editor as the main relationship-authoring surface in v1.0.1.

---

# 73. Entity vs Lore vs Graph

The three Orbis workflows serve different purposes.

## Entity Editor

Use when you want to work on:

```text
Entity
├── Identity
└── Properties
```

## Lore Editor

Use when you want to work on:

```text
Lore
├── Entities
└── Relationships
```

## Graph Viewer

Use when you want to:

```text
Visualize
Inspect
Navigate
```

the existing Lore graph.

This separation keeps the application easier to understand and avoids duplicating relationship management across multiple editors.

---

# 74. Recommended Orbis Workflow

A typical workflow can be:

### Step 1 — Create an Entity

Create the entity in the Entity Editor.

Example:

```text
Name: Avalon
Type: Character
```

### Step 2 — Add Properties

Add structured properties such as:

```text
title = King
age = 42
```

### Step 3 — Create or Open a Lore

Open the Lore Editor.

### Step 4 — Add Existing Entities

Select the existing entities that belong to the Lore.

### Step 5 — Establish Relationships

Create directed relationships between the entities.

Example:

```text
Avalon ── husband_of ──> Monami
```

### Step 6 — Save the Lore

Save the `.lore` document.

### Step 7 — View the Graph

Select **View Graph**.

### Step 8 — Inspect

Click an entity or relationship to inspect it.

---

# 75. Windows File Associations

The v1.0.1 installer registers:

```text
.md
.entity
.lore
```

with Orbpad.

This allows the files to be opened directly from Windows.

For example:

```text
Kingdom.md
Avalon.entity
Asterra.lore
```

can be associated with Orbpad.

---

# 76. Toolbar

The toolbar provides quick access to common operations.

Depending on the current configuration, the toolbar can provide access to:

- New
- Open
- Save
- Undo
- Redo
- Find
- Explorer

Use:

**View → Show Toolbar**

to show or hide the toolbar.

---

# 77. Status Bar

The status bar displays information about the current document.

Typical information includes:

- Line
- Column
- Word count
- Character count

Use:

**View → Show Status Bar**

to show or hide it.

---

# 78. Configuration

Orbpad stores application settings including:

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

# 79. About Orbpad

Open:

**Help → About Orbpad**

The About dialog displays information about the application, including version and developer information.

For v1.0.1:

```text
Orbpad v1.0.1
September 2026
```

Developer:

```text
Subhradeep Sarkar
```

---

# 80. Keyboard Shortcuts

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

# 81. Troubleshooting

## Orbpad does not start

Try launching Orbpad again.

If the installed application appears damaged, reinstall Orbpad using the official installer.

Because the official release is self-contained, a separate .NET runtime installation is not required.

---

## A file does not open

Check that:

- The file exists.
- You have permission to access it.
- The file is supported.
- The file is not inaccessible or locked.

For Orbis files, verify that the file has the correct extension:

```text
.entity
.lore
```

---

## Markdown preview is not visible

Open:

**View → Markdown**

and select:

- Split
- Preview

---

## Explorer is hidden

Use the **Explorer** toolbar control to restore it.

---

## Explorer is too narrow

Drag the Explorer splitter toward the right.

---

## An Entity does not appear as expected

Verify that:

- The `.entity` file can be opened.
- The entity has a valid name and type.
- The entity has not been accidentally duplicated.
- The correct existing entity was selected when working in the Lore Editor.

---

## A relationship is not visible in the graph

Verify that:

- The relationship was created in the current Lore.
- The Lore was saved after the relationship was created.
- Both source and target entities are present in the Lore graph.
- The Graph Viewer was reopened or refreshed after the graph changed.

---

## Two reverse relationships appear close together

Bidirectional relationships are intentionally rendered on separate lanes.

For example:

```text
A → B
B → A
```

should not share the same visual path.

If the relationship labels are difficult to distinguish, use the Relationship Inspector to inspect each relationship directly.

---

## Lore changes are not being retained

Make sure the Lore was saved after editing.

When switching between tabs or editors, Orbpad preserves the active Lore editor state in memory, but unsaved changes should still be explicitly saved before closing the Lore.

---

## Save As did not use the expected filename

The Lore Save As workflow uses the current Lore title as a suggested filename when appropriate.

The resulting file uses the `.lore` extension.

---

# 82. Uninstallation

Orbpad can be removed using Windows installed-app management.

Open:

**Windows Settings → Apps → Installed apps**

Find:

**Orbpad**

and select **Uninstall**.

The Inno Setup installer creates the required uninstaller entry.

---

# 83. System Requirements

Orbpad v1.0.1 is distributed for:

```text
Windows 10
Windows 11
x64
```

The official release is self-contained.

A separate .NET runtime installation is not required for the standard release.

See [`REQUIREMENTS.md`](REQUIREMENTS.md) for the complete platform and hardware requirements.

---

# 84. Third-Party Software

Orbpad uses third-party open-source software components.

Important components include:

- Avalonia
- AvaloniaEdit
- TextMateSharp
- Markdig
- Avalonia WebView components
- Orb.Engine

See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for dependency and licensing information.

---

# 85. Orb.Engine

Orb.Engine is the reusable engine behind the Orbis structured-information functionality used by Orbpad.

Repository:

https://github.com/Phantom-Con-Artist/Orb

Orb.Engine is maintained separately from Orbpad.

Orbpad uses the engine for the structured graph model underlying Entities, Relationships, Lore, and graph visualization.

---

# 86. Release Scope of v1.0.1

Orbpad v1.0.1 focuses on establishing a stable foundation for the integrated Orbis workflow.

The release includes:

- Focused text editing
- Markdown editing and preview
- Syntax highlighting
- File Explorer
- Recent files
- Windows file associations
- Entity Editor
- Entity properties
- Lore Editor
- Existing entity selection in Lore
- Directed relationships
- Bidirectional relationship visualization
- Graph Viewer
- Entity Inspector
- Relationship Inspector
- Zoom and pan
- Graph reset and fit controls
- Windows installer and uninstaller

The release does **not** attempt to expose every possible graph-analysis operation.

Advanced graph algorithms, traversal tools, additional graph layouts, and other future Orbis capabilities may be introduced in later releases.

---

# 87. Recommended Release Installation

For most users, the recommended installation method is the official Windows installer.

The release artifact is:

```text
Orbpad-1.0.1-win-x64-setup.exe
```

The installer provides:

- Orbpad application installation
- Start Menu shortcut
- Optional Desktop shortcut
- Windows uninstaller
- `.md` association
- `.entity` association
- `.lore` association

The application itself is self-contained.

---

# 88. License

Orbpad is distributed under the MIT License.

Copyright © 2026 Subhradeep Sarkar.

See [`LICENSE`](LICENSE) for the complete license text.

Third-party software remains subject to its respective license terms.

---

# 89. Further Documentation

For additional information, see:

- [`README.md`](README.md)
- [`REQUIREMENTS.md`](REQUIREMENTS.md)
- [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)
- [`CHANGELOG.md`](CHANGELOG.md)
- [`LICENSE`](LICENSE)

For the engine itself:

**Orb.Engine**

https://github.com/Phantom-Con-Artist/Orb

---

# 90. Version Information

```text
Orbpad
Version 1.0.1
Released September 2026
Platform Windows x64
Deployment Self-contained
Developer Subhradeep Sarkar
License MIT
```

---

<div align="center">

```text
Orbpad · v1.0.1
Windows x64 · Self-contained · MIT License
Part of the Orbis Ecosystem
```

</div>
