# Orbpad User Manual

## Version 1.0.0

**Released:** August 2026  
**Developer:** Phantom Con Artist

---

## 1. Introduction

Orbpad is a lightweight desktop text editor for Windows, designed for focused work with plain text, source code, and Markdown.

Orbpad provides:

- Plain-text editing
- Markdown editing and preview
- Syntax highlighting
- Multiple document tabs
- File Explorer
- Drag-and-drop file opening
- Find and Replace
- Themes
- Recent files
- Windows Open With integration
- Image viewing

---

## 2. Main Window

The Orbpad interface contains:

- Menu bar
- Toolbar
- Document tabs
- Editor
- File Explorer
- Markdown preview
- Search and Replace bar
- Status bar

The File Explorer can be resized or hidden to provide more space for editing.

---

## 3. Creating a Document

Select:

**File → New**

or press:

```text
Ctrl + N
```

Orbpad creates a new untitled document in a new tab.

You can also use the `+` button beside the document tabs.

---

## 4. Opening a File

Select:

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

---

## 5. Saving a File

To save the current document:

**File → Save**

or:

```text
Ctrl + S
```

For an unsaved document, Orbpad opens the Save As workflow.

To choose a filename or location manually:

**File → Save As**

or:

```text
Ctrl + Shift + S
```

---

## 6. Document Tabs

Orbpad supports multiple open documents.

Each document appears as a separate tab.

The active document is highlighted.

### Switch to the next document

```text
Ctrl + Tab
```

### Switch to the previous document

```text
Ctrl + Shift + Tab
```

### Switch directly to documents 1–9

```text
Ctrl + 1
Ctrl + 2
Ctrl + 3
...
Ctrl + 9
```

---

## 7. Closing a Document

To close the active document:

```text
Ctrl + W
```

If the document contains unsaved changes, Orbpad asks whether the changes should be saved.

---

## 8. Editing

Orbpad provides standard editing operations:

- Undo
- Redo
- Cut
- Copy
- Paste
- Select All

These are also available through the **Edit** menu.

---

## 9. Word Wrap

Word Wrap determines whether long lines wrap inside the editor.

Use:

**View → Word Wrap**

to enable or disable it.

---

## 10. Line Numbers

Use:

**View → Show Line Numbers**

to show or hide line numbers in the editor.

---

## 11. Fonts

Orbpad supports the following editor fonts:

- Inter
- Segoe UI
- Consolas
- Courier New

Open:

**View → Font**

to select a font.

---

## 12. Font Size

Open:

**View → Font Size**

Available sizes:

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

## 13. Markdown

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

---

## 14. Markdown Modes

Open:

**View → Markdown**

Orbpad provides three Markdown modes:

### Edit

Displays the Markdown source.

### Split

Displays the Markdown editor and rendered preview together.

### Preview

Displays the rendered Markdown document.

---

## 15. Markdown Preview

Markdown preview renders Markdown documents into a formatted view.

Orbpad uses Markdig for Markdown parsing and rendering.

---

## 16. Syntax Highlighting

Orbpad provides syntax highlighting for supported source files.

The appropriate language grammar is selected according to the opened file when supported.

---

## 17. File Explorer

Orbpad includes a File Explorer on the left side of the application.

The Explorer provides a hierarchical view of files and folders inside the selected workspace.

---

## 18. Opening a Workspace

Use the **Open Folder** control in the File Explorer.

After selecting a folder, its contents are displayed in the Explorer.

Files can then be opened directly from the Explorer.

---

## 19. Resizing the Explorer

The File Explorer can be resized horizontally.

Position the pointer over the boundary between the Explorer and editor.

Drag:

```text
←  to make the Explorer narrower

→  to make the Explorer wider
```

The editor automatically adjusts to the available space.

---

## 20. Hiding the Explorer

Use the Explorer collapse button to hide the Explorer.

When hidden, the editor uses the available horizontal space.

Use the main **Explorer** toolbar button to restore it.

---

## 21. Drag and Drop

Files can be dragged from Windows File Explorer into Orbpad.

Drop a supported file onto the Orbpad window to open it.

This provides a quick alternative to the Open dialog.

---

## 22. Recent Files

Orbpad keeps a list of recently opened files.

Open:

**File → Recent Files**

to access them.

Files that no longer exist are removed from the usable recent-file list.

---

## 23. Find

Use:

**Find**

from the toolbar or the appropriate menu command.

The search interface provides:

- Search
- Previous
- Next
- Close

Searches are case-insensitive.

---

## 24. Replace

The Find interface also provides:

- Replace
- Replace All

### Replace

Replaces the current matching occurrence.

### Replace All

Replaces all matching occurrences in the current document.

---

## 25. Images

Orbpad can open supported image files using:

**File → Open Image**

The image is displayed in Orbpad's image viewer.

---

## 26. Themes

Open:

**View → Themes**

Orbpad v1.0.0 provides:

- Orbpad Purple
- Orbpad Dark
- Midnight
- Forest
- Light

---

## 27. Orbpad Purple

**Orbpad Purple** is the default theme for a fresh Orbpad configuration.

It uses a dark violet interface with purple accent colors.

---

## 28. Orbpad Dark

**Orbpad Dark** provides a neutral dark interface with charcoal surfaces and violet accents.

---

## 29. Midnight

**Midnight** uses a deep blue-dark interface with blue accent colors.

---

## 30. Forest

**Forest** uses dark green surfaces and green accent colors.

---

## 31. Light

**Light** provides a bright interface with a light background and purple accent colors.

---

## 32. Windows Open With

Orbpad integrates with Windows file associations.

Markdown files can be opened using Orbpad through Windows.

For example:

**Right-click a `.md` file → Open With → Orbpad**

Windows may also allow Orbpad to be selected as the default application for Markdown files.

---

## 33. Toolbar

The toolbar provides quick access to common operations:

- New
- Open
- Save
- Undo
- Redo
- Find
- Explorer

The toolbar can be shown or hidden using:

**View → Show Toolbar**

---

## 34. Status Bar

The status bar displays information about the current document:

- Line
- Column
- Word count
- Character count

Use:

**View → Show Status Bar**

to show or hide it.

---

## 35. About Orbpad

Open:

**Help → About Orbpad**

The About dialog displays:

- Orbpad version
- Release date
- Developer
- Feature information
- Technology information
- License information

### Release

```text
Orbpad v1.0.0
August 2026
```

### Developer

```text
Phantom Con Artist
```

---

## 36. Keyboard Shortcuts

| Action | Shortcut |
|---|---|
| New document | Ctrl + N |
| Open file | Ctrl + O |
| Save | Ctrl + S |
| Save As | Ctrl + Shift + S |
| Undo | Ctrl + Z |
| Redo | Ctrl + Y |
| Cut | Ctrl + X |
| Copy | Ctrl + C |
| Paste | Ctrl + V |
| Select All | Ctrl + A |
| Close document | Ctrl + W |
| Next document | Ctrl + Tab |
| Previous document | Ctrl + Shift + Tab |
| Document 1–9 | Ctrl + 1–9 |

---

## 37. Configuration

Orbpad saves application settings including:

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

## 38. Troubleshooting

### Orbpad does not start

Try launching Orbpad again.

If the installed application appears damaged, reinstall Orbpad using the official installer.

### A file does not open

Check that:

- The file exists.
- You have permission to access it.
- The file is supported.
- The file is not inaccessible or locked.

### Markdown preview is not visible

Open:

**View → Markdown**

and select:

- Split
- Preview

### Explorer is hidden

Use the **Explorer** toolbar control to restore it.

### Explorer is too narrow

Drag the Explorer splitter toward the right.

### Markdown Open With is not working

Open Windows default-app settings and select Orbpad for Markdown files if necessary.

---

## 39. Uninstallation

Orbpad can be removed using Windows installed-app management.

Open:

**Windows Settings → Apps → Installed apps**

Find:

**Orbpad**

and select **Uninstall**.

Orbpad's installer provides an uninstaller entry for the application.

---

## 40. License

Orbpad is distributed under the MIT License.

Copyright © 2026 Phantom Con Artist.

See [`LICENSE`](LICENSE) for the complete license text.

---

## 41. Third-Party Software

Orbpad uses third-party open-source software components.

See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for information about third-party libraries and their respective licenses.

---

## 42. Version Information

```text
Orbpad
Version 1.0.0
Released August 2026
Platform Windows x64
Developer Phantom Con Artist
License MIT
```

---

## 43. Further Documentation

For more information:

- [`README.md`](README.md)
- [`REQUIREMENTS.md`](REQUIREMENTS.md)
- [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)
- [`CHANGELOG.md`](CHANGELOG.md)
- [`LICENSE`](LICENSE)
