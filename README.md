# ScreenDrop

A lightweight Windows desktop application for capturing screenshots, uploading files, and sharing text/code snippets to DigitalOcean Spaces with automatic URL copying. Built with .NET 8 and WPF.

## Features

- **Screenshot Capture**
  - Full screen capture with configurable hotkey (default: `Ctrl+Shift+S`)
  - Region selection capture with configurable hotkey (default: `Ctrl+Shift+R`)
  - Double-click tray icon for quick full screen capture

- **File Upload**
  - Upload any file via tray menu
  - Automatic upload to DigitalOcean Spaces
  - Public URL automatically copied to clipboard
  - Visual upload status indicator (blue = idle, orange = uploading)

- **Pastebin/Text Sharing**
  - Upload text, code, logs, or any text content
  - Syntax highlighting with Prism.js (200+ languages)
  - Line numbers with perfect alignment
  - Word wrap toggle in the viewer (no re-upload needed)
  - Custom URL slugs (e.g., `my-error-log.html`)
  - Short random IDs (e.g., `a3f9k2.html`)
  - Dark theme with GitHub-inspired design
  - Copy to clipboard, download raw, and word wrap buttons
  - Fully responsive for mobile and desktop
  - Configurable paste folder

- **Clipboard Monitoring**
  - Optional automatic upload when images are copied to clipboard
  - Works with Snipping Tool, Paint, browsers, and File Explorer
  - Instant URL generation for quick sharing

- **Filename Customization**
  - Template variables: `{date}`, `{time}`, `{timestamp}`, `{random}`, `{type}`
  - Example: `cap_{date}_{random}` becomes `cap_2026-02-28_a3f9k2.png`
  - Option to preserve original filenames

- **Upload History**
  - SQLite database tracking all uploads
  - Search and filter functionality
  - Thumbnail previews for images
  - Actions: Copy URL, Open in browser, Delete from Spaces
  - Configurable auto-deletion of old records (0 = keep forever)

- **System Tray Integration**
  - Runs silently in the background
  - Right-click menu for quick access
  - Notification system for upload confirmation

## Screenshots

*Coming soon*

## Requirements

- Windows 10/11
- DigitalOcean Spaces account (S3-compatible storage)

## Installation

1. Download the latest `ScreenDrop.exe` from [Releases](../../releases)
2. Run the executable
3. Configure your DigitalOcean Spaces credentials in Settings
4. Start capturing and uploading!

## Setup

### DigitalOcean Spaces Configuration

1. Create a Space in your [DigitalOcean account](https://cloud.digitalocean.com/spaces)
2. Generate API keys (Access Key and Secret Key) from the API section
3. Note your Space name and region (e.g., `nyc3`, `sfo3`, `sgp1`)
4. Ensure your Space has public read permissions
5. (Optional) Configure a custom CDN domain

### First Run

1. Right-click the ScreenDrop tray icon
2. Select "Settings"
3. Enter your DigitalOcean Spaces credentials:
   - Access Key
   - Secret Key
   - Bucket Name
   - Region
   - (Optional) Custom Domain
4. Configure your preferred settings:
   - Hotkeys
   - Filename templates
   - Upload folders (screenshots, files, pastes)
   - Clipboard monitoring
   - History retention
5. Click "Save & Close"

## Usage

### Taking Screenshots

- **Full Screen**: Press `Ctrl+Shift+S` (or your configured hotkey)
- **Region Selection**: Press `Ctrl+Shift+R` (or your configured hotkey)
- **Quick Capture**: Double-click the tray icon

### Uploading Files

1. Right-click the ScreenDrop tray icon
2. Select "Upload File..."
3. Choose a file
4. Public URL will be copied to clipboard automatically

### Uploading Text/Pastes

1. Right-click the ScreenDrop tray icon
2. Select "Upload Text/Paste..."
3. Enter your text/code/log content
4. (Optional) Add a title
5. (Optional) Enter a custom URL slug for a readable URL
6. Enable/disable syntax highlighting and line numbers
7. Click "Upload"
8. Public URL will be copied to clipboard automatically
9. Share the URL - viewers can copy, download, or toggle word wrap

**Examples:**
- Share error logs with developers
- Share code snippets with colleagues
- Share configuration files
- Share IRC/chat logs

### Clipboard Upload

When clipboard monitoring is enabled:
1. Copy any image (from Snipping Tool, Paint, browser, etc.)
2. ScreenDrop automatically uploads it
3. Public URL replaces the clipboard content
4. Notification confirms the upload

### Managing History

1. Right-click tray icon → Settings → History tab
2. View all your uploads with thumbnails
3. Search by filename or URL
4. Actions available:
   - **Copy URL**: Copy the public URL to clipboard
   - **Open**: Open the file in your browser
   - **Delete from Spaces**: Permanently remove from cloud storage

## Configuration Files

All settings and data are stored in your AppData folder:

- **Settings**: `%APPDATA%\ScreenDrop\settings.json`
- **Database**: `%APPDATA%\ScreenDrop\uploads.db`

This allows you to share the `ScreenDrop.exe` file without exposing your credentials.

## Filename Templates

Customize how your files are named using these variables:

| Variable | Description | Example |
|----------|-------------|---------|
| `{date}` | Current date | `2026-02-28` |
| `{time}` | Current time | `14-30-45` |
| `{timestamp}` | Unix timestamp | `1798675845` |
| `{random}` | Random 6-char string | `a3f9k2` |
| `{type}` | File type | `screenshot` or `file` |

**Example**: `cap_{date}_{random}` → `cap_2026-02-28_a3f9k2.png`

## Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

### Build Steps

```bash
# Clone the repository
git clone https://github.com/yourusername/ScreenDrop.git
cd ScreenDrop

# Build single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

# Output will be at:
# ScreenDrop\bin\Release\net8.0\win-x64\publish\ScreenDrop.exe
```

See [BUILD.md](BUILD.md) for detailed build instructions.

## Architecture

- **Framework**: .NET 8 WPF
- **Storage**: DigitalOcean Spaces (S3-compatible API)
- **Database**: SQLite
- **UI**: WPF with MVVM pattern

### Project Structure

```
ScreenDrop/
├── Models/              # Data models
├── Services/            # Business logic
├── ViewModels/          # MVVM view models
├── Views/               # XAML UI
├── Helpers/             # Utility classes
└── Converters/          # WPF value converters
```

## Known Limitations

- Windows does not support drag & drop to system tray icons
- Clipboard monitoring requires the app to be running
- Multi-monitor support tested on Windows 10/11

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - feel free to use this project however you'd like.

## Acknowledgments

- Built with [AWSSDK.S3](https://www.nuget.org/packages/AWSSDK.S3/) for DigitalOcean Spaces integration
- Uses [Hardcodet.NotifyIcon.Wpf](https://www.nuget.org/packages/Hardcodet.NotifyIcon.Wpf/) for system tray functionality
- Database management with [Dapper](https://www.nuget.org/packages/Dapper/) and [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite/)
- Syntax highlighting powered by [Prism.js](https://prismjs.com/)

## Support

If you encounter any issues or have feature requests, please [open an issue](../../issues).
