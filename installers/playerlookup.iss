#ifndef BuildOutput
  #define BuildOutput "bin\Release\net8.0-windows\win-x64"
#endif

[Setup]
AppName=PlayerLookup
AppVersion=1.0.0
AppPublisher=oasisdfg
DefaultDirName={autopf}\xhub\PlayerLookup
DefaultGroupName=xhub
OutputDir=Output
OutputBaseFilename=playerlookup_setup
Compression=lzma
SolidCompression=yes
UninstallDisplayIcon={app}\PlayerLookup.exe
PrivilegesRequired=lowest
DisableProgramGroupPage=yes

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "{#BuildOutput}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\PlayerLookup"; Filename: "{app}\PlayerLookup.exe"
Name: "{autodesktop}\PlayerLookup"; Filename: "{app}\PlayerLookup.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\PlayerLookup.exe"; Description: "Launch PlayerLookup"; Flags: nowait postinstall skipifsilent
