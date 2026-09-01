# Changelog

All notable changes to Orbpad are documented in this file.

Orbpad follows semantic versioning principles:

```text
MAJOR.MINOR.PATCH
```

---

# [1.0.1] — September 2026

Orbpad v1.0.1 is the first release focused on integrating Orbpad with the Orbis structured-information workflow through **Orb.Engine**.

This release expands Orbpad beyond a traditional text editor while keeping the core editing experience intact.

## Orbis Integration

### Orb.Engine Integration

- Integrated Orbpad with the **Orb.Engine** project.
- Added the Orb.Engine project as a project dependency.
- Orbpad now uses the shared Orbis graph and structured-information model for Lore functionality.
- Added documentation links to the separate Orb.Engine repository:
  https://github.com/Phantom-Con-Artist/Orb

### Entity Support

- Added integrated Entity Editor workflow.
- Added Entity identity editing:
  - Name
  - Type
  - Stable ID
- Added Entity property management.
- Added property creation.
- Added property editing.
- Added property removal.
- Added `.entity` workflow support.
- Entity Editor intentionally focuses on entity identity and properties rather than duplicating relationship management.

### Lore Support

- Added integrated Lore Editor workflow.
- Added creation of new Lore documents.
- Added opening of existing `.lore` documents.
- Added Lore title handling based on the saved filename.
- Added preservation of the active Lore editor state while switching between documents.
- Added Lore Save and Save As workflows.
- Added Lore dirty-state protection for unsaved changes.
- Added support for working with existing entities inside a Lore.
- Added relationship management inside the Lore Editor.

### Relationships

- Added directed relationship creation.
- Added relationship editing.
- Added source/target entity selection.
- Added relationship type handling.
- Added relationship persistence as part of the Lore graph.
- Added support for relationships pointing in both directions between the same entities.
- Improved visual separation of multiple relationships between the same pair of entities.
- Added relationship labels in the graph.

---

## Graph Viewer

### Dedicated Graph Window

- Added a standalone **Graph Viewer** window.
- Graph visualization is separated from the main Orbpad editor workspace.
- Lore Editor can launch the Graph Viewer directly.
- Graph Viewer operates on the current Lore graph.

### Graph Visualization

- Added entity nodes.
- Added directed relationship rendering.
- Added directional arrowheads.
- Added relationship labels.
- Added self-relationship rendering.
- Added dedicated handling for bidirectional relationships.
- Added separated relationship lanes for multiple connections between the same entities.
- Added styled entity nodes with gradient surfaces.
- Added node hover feedback.
- Added visual relationship glow effects.
- Added animated graph view transitions.

### Graph Navigation

- Added graph panning.
- Added mouse-wheel zoom.
- Added pointer-centered zoom behavior.
- Added Reset View.
- Added Fit Graph.
- Added smooth animated reset/fit transitions.

### Entity Inspector

- Added Entity Inspector to the Graph Viewer.
- Selecting an entity displays:
  - Name
  - Type
  - ID
  - Properties

### Relationship Inspector

- Added Relationship Inspector to the Graph Viewer.
- Selecting a relationship displays:
  - Relationship type
  - Source entity
  - Target entity
  - Relationship ID
  - Relationship properties

### Graph Selection

- Entity nodes can be selected directly.
- Relationship labels can be selected directly.
- Clicking empty graph space clears the current inspector selection.

---

## Entity Editor

- Removed the dedicated Relationships panel from the Entity Editor.
- Relationship management is now intentionally centralized in the Lore Editor.
- Preserved the existing Entity property editing workflow.
- Improved separation of responsibilities between Entity Editor and Lore Editor.

The v1.0.1 model is:

```text
Entity Editor
    ↓
Entity identity + properties

Lore Editor
    ↓
Entities + relationships

Graph Viewer
    ↓
Visualization + inspection
```

---

## File Handling

- Added `.entity` support to the structured-information workflow.
- Added `.lore` support to the structured-information workflow.
- Preserved `.md` Markdown support.
- Improved Windows Open With integration through the installer.
- Added Windows file associations for:
  - `.md`
  - `.entity`
  - `.lore`

---

## Lore Saving

- Improved Lore Save handling.
- Improved Lore Save As behavior.
- Saving a new Lore establishes its file path.
- Saved Lore titles are synchronized with their filenames.
- Save As updates the current Lore path to the newly saved file.
- Improved Lore document/tab synchronization.
- Prevented Save As from creating an unrelated empty Lore document.
- Preserved graph state when switching back to an already-open Lore.

---

## UI and Stability

- Improved Lore Editor layout and presentation.
- Improved Graph Viewer visual design.
- Added a more polished graph presentation with gradients, shadows, glow, hover behavior, and animated navigation.
- Improved editor switching between normal documents, Entity Editor, and Lore Editor.
- Removed obsolete relationship UI from the Entity Editor.
- Cleaned unused Lore editor state.
- Resolved Avalonia compiled-binding issues encountered during Lore Editor development.
- Resolved Graph Viewer geometry and nullable-reference warnings.
- Resolved the remaining application compiler warnings for the Orbpad release build.

---

## Installer

- Updated Inno Setup installer from v1.0.0 to v1.0.1.
- Installer output is now:

```text
Orbpad-1.0.1-win-x64-setup.exe
```

- Installer uses the self-contained Windows x64 publish.
- Updated installer version metadata to `1.0.1`.
- Updated application version metadata to `1.0.1`.
- Added Windows associations for:
  - `.md`
  - `.entity`
  - `.lore`
- Preserved:
  - Start Menu shortcut
  - Optional Desktop shortcut
  - Windows uninstaller
  - Orbpad application icon
  - Post-install launch option
- Installer publisher metadata updated to the current project developer identity.

---

## Release

- Updated Orbpad version from `1.0.0` to `1.0.1`.
- Added explicit application version metadata:
  - Version: `1.0.1`
  - Assembly Version: `1.0.1.0`
  - File Version: `1.0.1.0`
  - Informational Version: `1.0.1`
- Verified clean Release build.
- Verified self-contained Windows x64 publish.
- Verified published application launch and core workflows.
- Verified Inno Setup compilation.
- Verified installation and uninstallation.
- Verified Windows file associations.

---

# [1.0.0] — August 2026

Initial stable release of Orbpad.

## Editor

- Plain-text editing
- Source-code editing
- Multiple document tabs
- Undo and Redo
- Cut, Copy, Paste, and Select All
- Word Wrap
- Line numbers
- Configurable editor fonts
- Configurable editor font sizes

## Markdown

- Markdown editing
- Markdown preview
- Split Markdown editing and preview
- Markdown rendering powered by Markdig

## Syntax Highlighting

- TextMate-based syntax highlighting
- Automatic language selection for supported file types

## File Explorer

- Built-in workspace File Explorer
- Folder navigation
- File opening from Explorer
- Resizable Explorer
- Hideable Explorer
- Explorer restore control
- Main toolbar Explorer control

## File Management

- Open files
- Save
- Save As
- Recent Files
- Drag-and-drop file opening
- Windows Open With integration
- Markdown file association support
- Image opening and viewing

## Search and Replace

- Find
- Find Previous
- Find Next
- Replace
- Replace All
- Case-insensitive search

## Themes

- Orbpad Purple
- Orbpad Dark
- Midnight
- Forest
- Light

### Orbpad Purple

- Introduced as the default Orbpad theme
- Dark violet interface
- Purple accent palette

## Customization

- Inter font support
- Segoe UI support
- Consolas support
- Courier New support
- Multiple editor font sizes
- Toolbar visibility
- Status bar visibility
- Line number visibility
- Word Wrap setting

## Application

- Custom Orbpad application icon
- Windows executable icon
- Window and taskbar icon
- About Orbpad dialog
- Version information
- Developer information
- MIT License information

## Windows Integration

- Windows Open With integration
- Markdown file association support
- Start Menu shortcut through installer
- Optional Desktop shortcut
- Windows uninstaller

## Release

- Windows x64 release
- Self-contained .NET deployment
- Inno Setup installer

---

# Versioning

Orbpad follows semantic versioning principles:

```text
MAJOR.MINOR.PATCH
```

A patch release is intended for compatible bug fixes and release maintenance.

A minor release may introduce backwards-compatible functionality.

A major release may introduce breaking changes.

