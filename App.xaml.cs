using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using ScreenDrop.Helpers;
using ScreenDrop.Models;
using ScreenDrop.Services;
using ScreenDrop.Views;

namespace ScreenDrop;

public partial class App : Application
{
    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);
    
    private TaskbarIcon? _trayIcon;
    private Window? _mainWindow;
    private GlobalHotkey? _hotkey;
    private GlobalHotkey? _regionHotkey;
    private ClipboardService? _clipboardService;
    private IntPtr _iconHandle;
    private IntPtr _uploadingIconHandle;
    private bool _isUploading;
    
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;
    private readonly DatabaseService _databaseService;
    private AppSettings _settings;
    private bool _clipboardEnabled;

    public App()
    {
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();
        _notificationService = new NotificationService();
        _databaseService = new DatabaseService();
        
        // Clean up old upload records on startup (if retention is configured)
        if (_settings.HistoryRetentionDays > 0)
        {
            Task.Run(async () => await _databaseService.DeleteOldUploadsAsync(_settings.HistoryRetentionDays));
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mainWindow = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false
        };
        _mainWindow.Show();
        _mainWindow.Hide();

        SetupTray();
        SetupHotkeys();

        if (_settings.IsConfigured)
        {
            if (_clipboardEnabled)
            {
                StartClipboardMonitoring();
            }
        }
        else
        {
            ShowSettings();
        }
    }

    private void SetupTray()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "ScreenDrop",
            ContextMenu = (System.Windows.Controls.ContextMenu)FindResource("TrayMenu"),
            Visibility = Visibility.Visible
        };

        _trayIcon.TrayMouseDoubleClick += (_, _) => TakeScreenshot();

        try
        {
            _trayIcon.Icon = CreateTrayIcon();
        }
        catch
        {
        }
    }

    private System.Drawing.Icon CreateTrayIcon(bool uploading = false)
    {
        // Create a 16x16 bitmap for the icon
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        
        // Enable anti-aliasing for smoother shapes
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        // Fill background with transparent
        graphics.Clear(System.Drawing.Color.Transparent);
        
        // Draw camera shape with different colors for uploading state
        var cameraColor = uploading 
            ? System.Drawing.Color.FromArgb(255, 140, 0)  // Orange for uploading
            : System.Drawing.Color.FromArgb(0, 120, 215); // Blue for normal
        
        // Camera body
        using var cameraBrush = new SolidBrush(cameraColor);
        graphics.FillRectangle(cameraBrush, 2, 5, 12, 8);
        
        // Camera lens (white circle)
        using var whiteBrush = new SolidBrush(System.Drawing.Color.White);
        graphics.FillEllipse(whiteBrush, 5, 7, 6, 6);
        
        // Lens center
        var lensColor = uploading
            ? System.Drawing.Color.FromArgb(200, 100, 0)  // Dark orange
            : System.Drawing.Color.FromArgb(0, 80, 150);  // Dark blue
        using var lensBrush = new SolidBrush(lensColor);
        graphics.FillEllipse(lensBrush, 7, 9, 2, 2);
        
        // Viewfinder (small rectangle on top)
        graphics.FillRectangle(whiteBrush, 6, 3, 4, 2);
        
        // Add upload indicator (small circle in corner) when uploading
        if (uploading)
        {
            using var greenBrush = new SolidBrush(System.Drawing.Color.FromArgb(0, 200, 0));
            graphics.FillEllipse(greenBrush, 11, 1, 4, 4);
        }
        
        // Convert to icon and store handle
        var handle = bitmap.GetHicon();
        if (uploading)
            _uploadingIconHandle = handle;
        else
            _iconHandle = handle;
        
        var icon = System.Drawing.Icon.FromHandle(handle);
        
        return icon;
    }

    private void SetUploadingState(bool uploading)
    {
        if (_isUploading == uploading) return;
        
        _isUploading = uploading;
        
        try
        {
            if (_trayIcon != null)
            {
                _trayIcon.Icon = CreateTrayIcon(uploading);
                _trayIcon.ToolTipText = uploading 
                    ? "ScreenDrop - Uploading..." 
                    : "ScreenDrop";
            }
        }
        catch
        {
            // Ignore icon update errors
        }
    }

    private void SetupHotkeys()
    {
        if (_mainWindow == null) return;

        try
        {
            var (modifiers, key) = ParseHotkey(_settings.FullScreenHotkey);
            _hotkey = new GlobalHotkey(_mainWindow, 1, modifiers, key);
            _hotkey.HotkeyPressed += (_, _) => TakeScreenshot();
        }
        catch
        {
            _trayIcon?.ShowBalloonTip("ScreenDrop", $"Could not register {_settings.FullScreenHotkey} hotkey", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
        }

        try
        {
            var (modifiers, key) = ParseHotkey(_settings.RegionHotkey);
            _regionHotkey = new GlobalHotkey(_mainWindow, 2, modifiers, key);
            _regionHotkey.HotkeyPressed += (_, _) => SelectRegion();
        }
        catch
        {
            _trayIcon?.ShowBalloonTip("ScreenDrop", $"Could not register {_settings.RegionHotkey} hotkey", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
        }
    }

    private (Helpers.HotkeyModifiers, Key) ParseHotkey(string hotkey)
    {
        var parts = hotkey.Split('+');
        var modifiers = Helpers.HotkeyModifiers.None;
        Key key = Key.None;

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            switch (trimmed.ToLower())
            {
                case "ctrl":
                case "control":
                    modifiers |= Helpers.HotkeyModifiers.Control;
                    break;
                case "shift":
                    modifiers |= Helpers.HotkeyModifiers.Shift;
                    break;
                case "alt":
                    modifiers |= Helpers.HotkeyModifiers.Alt;
                    break;
                case "win":
                case "windows":
                    modifiers |= Helpers.HotkeyModifiers.Win;
                    break;
                default:
                    // Try to parse as a key
                    if (Enum.TryParse<Key>(trimmed, true, out var parsedKey))
                    {
                        key = parsedKey;
                    }
                    break;
            }
        }

        return (modifiers, key);
    }

    private static string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLower() switch
        {
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "webp" => "image/webp",
            "mp4" => "video/mp4",
            "webm" => "video/webm",
            "pdf" => "application/pdf",
            "txt" => "text/plain",
            "zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    private void StartClipboardMonitoring()
    {
        if (_mainWindow == null) return;
        
        _clipboardService = new ClipboardService(OnClipboardImage);
        _clipboardService.StartMonitoring(_mainWindow);
    }

    private void OnClipboardImage(byte[] imageData)
    {
        _ = UploadImageAsync(imageData, "Clipboard");
    }

    private async void TakeScreenshot()
    {
        if (!_settings.IsConfigured)
        {
            ShowSettings();
            return;
        }

        await Task.Delay(100);
        
        var capture = new CaptureService();
        var imageData = capture.CaptureFullScreen();
        
        await UploadImageAsync(imageData);
    }

    private async void SelectRegion()
    {
        if (!_settings.IsConfigured)
        {
            ShowSettings();
            return;
        }

        var selector = new RegionSelector();
        if (selector.ShowDialog() == true && selector.SelectedRegion.HasValue)
        {
            var region = selector.SelectedRegion.Value;
            var capture = new CaptureService();
            var imageData = capture.CaptureRegion(
                (int)region.X, (int)region.Y, 
                (int)region.Width, (int)region.Height);
            
            await UploadImageAsync(imageData);
        }
    }

    private async Task UploadImageAsync(byte[] imageData, string uploadType = "Screenshot")
    {
        try
        {
            SetUploadingState(true);
            
            var service = new DigitalOceanService(_settings);
            var extension = "png";
            var folder = _settings.ScreenshotFolder;
            
            // Upload and get the actual S3 key that was used
            var result = await service.UploadImageAsync(imageData, extension, folder);
            
            // Generate thumbnail
            var thumbnail = DigitalOceanService.GenerateThumbnail(imageData);
            
            // Save to database with the ACTUAL S3Key from the upload
            var record = new UploadRecord
            {
                FileName = result.FileName,
                Url = result.Url,
                S3Key = result.S3Key,
                Folder = folder,
                FileSize = imageData.Length,
                ContentType = "image/png",
                UploadDate = DateTime.Now,
                UploadType = uploadType,
                ThumbnailData = thumbnail
            };
            
            await _databaseService.AddUploadRecordAsync(record);
            
            Clipboard.SetText(result.Url);
            _trayIcon?.ShowBalloonTip("Screenshot Uploaded!", "URL copied to clipboard", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
        catch (Exception ex)
        {
            _trayIcon?.ShowBalloonTip("Upload Failed", ex.Message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
        }
        finally
        {
            SetUploadingState(false);
        }
    }

    private void UploadFile()
    {
        if (!_settings.IsConfigured)
        {
            ShowSettings();
            return;
        }

        // Use Dispatcher to ensure we're on the UI thread and dialog works properly
        Current.Dispatcher.Invoke(async () =>
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select File to Upload",
                Filter = "All Files (*.*)|*.*|Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|Videos (*.mp4;*.webm)|*.mp4;*.webm",
                FilterIndex = 1,
                CheckFileExists = true
            };

            var dialogResult = dialog.ShowDialog(_mainWindow);
            
            if (dialogResult == true)
            {
                try
                {
                    SetUploadingState(true);
                    
                    var service = new DigitalOceanService(_settings);
                    var folder = _settings.FileFolder;
                    var filePath = dialog.FileName;
                    
                    // Upload and get the actual S3 key that was used
                    var uploadResult = await service.UploadFileAsync(filePath, folder, _settings.UseOriginalFilename);
                    
                    // Get file info
                    var fileInfo = new FileInfo(filePath);
                    var extension = Path.GetExtension(filePath).TrimStart('.').ToLower();
                    var contentType = GetContentTypeFromExtension(extension);
                    
                    // Generate thumbnail if image
                    byte[]? thumbnail = null;
                    if (contentType.StartsWith("image/"))
                    {
                        var imageData = await File.ReadAllBytesAsync(filePath);
                        thumbnail = DigitalOceanService.GenerateThumbnail(imageData);
                    }
                    
                    // Save to database with the ACTUAL S3Key from the upload
                    var record = new UploadRecord
                    {
                        FileName = uploadResult.FileName,
                        Url = uploadResult.Url,
                        S3Key = uploadResult.S3Key,
                        Folder = folder,
                        FileSize = fileInfo.Length,
                        ContentType = contentType,
                        UploadDate = DateTime.Now,
                        UploadType = "File",
                        ThumbnailData = thumbnail
                    };
                    
                    await _databaseService.AddUploadRecordAsync(record);
                    
                    Clipboard.SetText(uploadResult.Url);
                    
                    var originalFileName = System.IO.Path.GetFileName(filePath);
                    _trayIcon?.ShowBalloonTip("File Uploaded!", $"{originalFileName}\nURL copied to clipboard", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
                catch (Exception ex)
                {
                    _trayIcon?.ShowBalloonTip("Upload Failed", ex.Message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
                }
                finally
                {
                    SetUploadingState(false);
                }
            }
        });
    }

    private void ShowSettings()
    {
        var window = new SettingsWindow(_settingsService, _settings, _databaseService);
        
        if (_clipboardEnabled)
        {
            window.ClipboardMonitoringEnabled = true;
        }
        
        window.ShowDialog();
        
        _settings = _settingsService.Load();
        _clipboardEnabled = window.ClipboardMonitoringEnabled;
        
        // Reload hotkeys if they changed
        _hotkey?.Dispose();
        _regionHotkey?.Dispose();
        SetupHotkeys();
        
        if (_settings.IsConfigured && _clipboardEnabled)
        {
            StartClipboardMonitoring();
        }
    }

    private void TakeScreenshot_Click(object sender, RoutedEventArgs e)
    {
        TakeScreenshot();
    }

    private void SelectRegion_Click(object sender, RoutedEventArgs e)
    {
        SelectRegion();
    }

    private void UploadFile_Click(object sender, RoutedEventArgs e)
    {
        UploadFile();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ShowSettings();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _hotkey?.Dispose();
        _regionHotkey?.Dispose();
        _clipboardService?.StopMonitoring();
        _trayIcon?.Dispose();
        
        if (_iconHandle != IntPtr.Zero)
        {
            DestroyIcon(_iconHandle);
            _iconHandle = IntPtr.Zero;
        }
        
        if (_uploadingIconHandle != IntPtr.Zero)
        {
            DestroyIcon(_uploadingIconHandle);
            _uploadingIconHandle = IntPtr.Zero;
        }
        
        if (_mainWindow != null)
        {
            _mainWindow.Close();
        }
        
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkey?.Dispose();
        _regionHotkey?.Dispose();
        _clipboardService?.StopMonitoring();
        _trayIcon?.Dispose();
        
        if (_iconHandle != IntPtr.Zero)
        {
            DestroyIcon(_iconHandle);
            _iconHandle = IntPtr.Zero;
        }
        
        if (_uploadingIconHandle != IntPtr.Zero)
        {
            DestroyIcon(_uploadingIconHandle);
            _uploadingIconHandle = IntPtr.Zero;
        }
        
        base.OnExit(e);
    }
}
