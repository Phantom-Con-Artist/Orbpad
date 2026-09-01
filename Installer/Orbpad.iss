#define MyAppName "Orbpad"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "Subhradeep Sarkar"
#define MyAppExeName "Orbpad.exe"

[Setup]

AppId={{A7F5F4C1-5E4D-4A8F-9C0D-7B6E8F2D41A3}}

AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}

AppPublisher={#MyAppPublisher}

DefaultDirName={autopf}\Orbpad
DefaultGroupName=Orbpad

DisableProgramGroupPage=yes

OutputDir=..\Release
OutputBaseFilename=Orbpad-1.0.1-win-x64-setup

Compression=lzma
SolidCompression=yes

WizardStyle=modern

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

UninstallDisplayName=Orbpad
UninstallDisplayIcon={app}\Orbpad.exe

SetupIconFile=..\Assets\Orbpad.ico

PrivilegesRequired=admin

VersionInfoVersion=1.0.1.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Orbpad Text Editor
VersionInfoProductName=Orbpad
VersionInfoProductVersion=1.0.1
VersionInfoCopyright=Copyright © 2026 Phantom Con Artist

[Languages]

Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]

Source: "..\release\Orbpad-v1.0.1-win-x64\*"; \
    DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]

Name: "desktopicon"; \
    Description: "Create a desktop shortcut"; \
    GroupDescription: "Additional shortcuts:"; \
    Flags: unchecked

[Icons]

Name: "{autoprograms}\Orbpad"; \
    Filename: "{app}\Orbpad.exe"; \
    WorkingDir: "{app}"; \
    IconFilename: "{app}\Orbpad.exe"

Name: "{autodesktop}\Orbpad"; \
    Filename: "{app}\Orbpad.exe"; \
    WorkingDir: "{app}"; \
    IconFilename: "{app}\Orbpad.exe"; \
    Tasks: desktopicon

[Registry]

; ============================================================
; Markdown
; ============================================================

Root: HKCR; \
    Subkey: ".md\OpenWithProgids"; \
    ValueType: string; \
    ValueName: "Orbpad.md"; \
    ValueData: ""

Root: HKCR; \
    Subkey: "Orbpad.md"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "Markdown Document"

Root: HKCR; \
    Subkey: "Orbpad.md\DefaultIcon"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "{app}\Orbpad.exe,0"

Root: HKCR; \
    Subkey: "Orbpad.md\shell\open\command"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: """{app}\Orbpad.exe"" ""%1"""


; ============================================================
; Orb Entity
; ============================================================

Root: HKCR; \
    Subkey: ".entity\OpenWithProgids"; \
    ValueType: string; \
    ValueName: "Orbpad.entity"; \
    ValueData: ""

Root: HKCR; \
    Subkey: "Orbpad.entity"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "Orb Entity"

Root: HKCR; \
    Subkey: "Orbpad.entity\DefaultIcon"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "{app}\Orbpad.exe,0"

Root: HKCR; \
    Subkey: "Orbpad.entity\shell\open\command"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: """{app}\Orbpad.exe"" ""%1"""


; ============================================================
; Orb Lore
; ============================================================

Root: HKCR; \
    Subkey: ".lore\OpenWithProgids"; \
    ValueType: string; \
    ValueName: "Orbpad.lore"; \
    ValueData: ""

Root: HKCR; \
    Subkey: "Orbpad.lore"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "Orb Lore"

Root: HKCR; \
    Subkey: "Orbpad.lore\DefaultIcon"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "{app}\Orbpad.exe,0"

Root: HKCR; \
    Subkey: "Orbpad.lore\shell\open\command"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: """{app}\Orbpad.exe"" ""%1"""

[Run]

Filename: "{app}\Orbpad.exe"; \
    Description: "Launch Orbpad"; \
    Flags: nowait postinstall skipifsilent