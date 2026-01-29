; Moltbot Tray Inno Setup Script
#define MyAppName "Moltbot Tray"
#define MyAppPublisher "Scott Hanselman"
#define MyAppURL "https://github.com/shanselman/moltbot-windows-hub"
#define MyAppExeName "Moltbot.Tray.exe"

[Setup]
AppId={{M0LTB0T-TRAY-4PP1-D3N7}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL=https://github.com/shanselman/moltbot-windows-hub/issues
AppUpdatesURL=https://github.com/shanselman/moltbot-windows-hub/releases
DefaultDirName={localappdata}\MoltbotTray
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=MoltbotTray-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
SetupIconFile=src\Moltbot.Tray\moltbot.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Start Moltbot Tray when Windows starts"; GroupDescription: "Startup:"; Flags: unchecked
Name: "cmdpalette"; Description: "Install PowerToys Command Palette extension"; GroupDescription: "Integrations:"; Flags: unchecked

[Files]
; Tray app
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\Moltbot.Tray\moltbot.ico"; DestDir: "{app}"; Flags: ignoreversion
; Command Palette extension (all files from build output)
Source: "publish\cmdpal\*"; DestDir: "{app}\CommandPalette"; Flags: ignoreversion recursesubdirs; Tasks: cmdpalette

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\moltbot.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\moltbot.ico"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
; Register Command Palette extension (silently, only if task selected)
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Add-AppxPackage -Register '{app}\CommandPalette\AppxManifest.xml' -ForceApplicationShutdown"""; Flags: runhidden; Tasks: cmdpalette

[UninstallRun]
; Unregister Command Palette extension on uninstall
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Get-AppxPackage -Name '*Moltbot*' | Remove-AppxPackage"""; Flags: runhidden
