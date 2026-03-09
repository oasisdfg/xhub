#ifndef BuildOutput
  #define BuildOutput "bin\Release\net8.0-windows\win-x64"
#endif

[Setup]
AppName=stretchedres
AppVersion=1.0.0
AppPublisher=oasisdfg
DefaultDirName={autopf}\xhub\stretchedres
DefaultGroupName=xhub
OutputDir=Output
OutputBaseFilename=stretchedres_setup
Compression=lzma
SolidCompression=yes
; NOTE: SetupIconFile assumes the stretchedres repository is a sibling of the xhub repository.
; Adjust this path if your folder layout differs, or remove the line to use the default icon.
SetupIconFile=..\..\..\stretchedres\stretchedres\app.ico
UninstallDisplayIcon={app}\stretchedres.exe
PrivilegesRequired=lowest
DisableProgramGroupPage=yes

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "{#BuildOutput}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\stretchedres"; Filename: "{app}\stretchedres.exe"
Name: "{autodesktop}\stretchedres"; Filename: "{app}\stretchedres.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\stretchedres.exe"; Description: "Launch stretchedres"; Flags: nowait postinstall skipifsilent
