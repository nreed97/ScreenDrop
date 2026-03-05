# ScreenDrop

Windows screenshot/media uploader to DigitalOcean Spaces.

## Prerequisites

- Windows 10/11
- .NET 8 SDK
- DigitalOcean Spaces account

## Build

```powershell
cd ScreenDrop
dotnet restore
dotnet build
```

## Publish

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Output: `ScreenDrop/bin/Release/net8.0-windows/win-x64/publish/ScreenDrop.exe`

## First Run

1. Run `ScreenDrop.exe`
2. Right-click tray icon → Settings
3. Enter your DigitalOcean credentials:
   - Access Key (DO Spaces key)
   - Secret Key
   - Bucket Name
   - Region (e.g., `nyc3`)
   - Custom Domain (optional)

## Usage

| Action | Trigger |
|--------|---------|
| Full screenshot | `Ctrl+Shift+S` or double-click tray |
| Region select | `Ctrl+Shift+R` or tray menu |
| Settings | Right-click tray → Settings |
| Exit | Right-click tray → Exit |

When enabled, copied images are automatically uploaded.

## Features

- System tray residence
- Global hotkeys (Ctrl+Shift+S, Ctrl+Shift+R)
- Clipboard monitoring for auto-upload
- Region selection capture
- Silent upload + URL copied to clipboard
- Windows toast notifications
