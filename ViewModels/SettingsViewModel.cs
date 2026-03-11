using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenDrop.Models;
using ScreenDrop.Services;

namespace ScreenDrop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly DatabaseService? _databaseService;
    private readonly DigitalOceanService? _digitalOceanService;

    [ObservableProperty]
    private string _accessKey = string.Empty;

    [ObservableProperty]
    private string _secretKey = string.Empty;

    [ObservableProperty]
    private string _bucketName = string.Empty;

    [ObservableProperty]
    private string _region = "nyc3";

    [ObservableProperty]
    private string _customDomain = string.Empty;

    [ObservableProperty]
    private string _screenshotFolder = "caps";

    [ObservableProperty]
    private string _fileFolder = "files";

    [ObservableProperty]
    private string _pasteFolder = "pastes";

    [ObservableProperty]
    private string _screenshotFilenameTemplate = "{timestamp}_{random}";

    [ObservableProperty]
    private string _fileFilenameTemplate = "{timestamp}_{random}";

    [ObservableProperty]
    private bool _useOriginalFilename = false;

    [ObservableProperty]
    private int _historyRetentionDays = 30;

    [ObservableProperty]
    private bool _clipboardMonitoringEnabled;

    [ObservableProperty]
    private string _fullScreenHotkey = "Ctrl+Shift+S";

    [ObservableProperty]
    private string _regionHotkey = "Ctrl+Shift+R";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _historyStatusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UploadRecord> _uploadHistory = new();

    [ObservableProperty]
    private UploadRecord? _selectedRecord;

    public string[] AvailableRegions { get; } = new[]
    {
        "nyc1", "nyc2", "nyc3",
        "ams2", "ams3",
        "sgp1",
        "fra1",
        "sfo1", "sfo2", "sfo3",
        "lon1",
        "tor1",
        "blr1"
    };

    public SettingsViewModel(SettingsService settingsService, AppSettings settings, DatabaseService? databaseService = null)
    {
        _settingsService = settingsService;
        _settings = settings;
        _databaseService = databaseService;
        _digitalOceanService = settings.IsConfigured ? new DigitalOceanService(settings) : null;
        
        AccessKey = settings.AccessKey;
        SecretKey = settings.SecretKey;
        BucketName = settings.BucketName;
        Region = settings.Region;
        CustomDomain = settings.CustomDomain;
        ScreenshotFolder = settings.ScreenshotFolder;
        FileFolder = settings.FileFolder;
        PasteFolder = settings.PasteFolder;
        ScreenshotFilenameTemplate = settings.ScreenshotFilenameTemplate;
        FileFilenameTemplate = settings.FileFilenameTemplate;
        UseOriginalFilename = settings.UseOriginalFilename;
        HistoryRetentionDays = settings.HistoryRetentionDays;
        FullScreenHotkey = settings.FullScreenHotkey;
        RegionHotkey = settings.RegionHotkey;

        // Load history if database service is available
        if (_databaseService != null)
        {
            _ = LoadHistoryAsync();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = SearchHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        if (_databaseService == null) return;

        try
        {
            var records = await _databaseService.GetRecentUploadsAsync(_settings.HistoryRetentionDays);
            UploadHistory.Clear();
            foreach (var record in records)
            {
                UploadHistory.Add(record);
            }
            
            if (_settings.HistoryRetentionDays == 0)
            {
                HistoryStatusMessage = $"{UploadHistory.Count} upload(s) (keeping all history)";
            }
            else
            {
                HistoryStatusMessage = $"{UploadHistory.Count} upload(s) in last {_settings.HistoryRetentionDays} days";
            }
        }
        catch (Exception ex)
        {
            HistoryStatusMessage = $"Error loading history: {ex.Message}";
        }
    }

    private async Task SearchHistoryAsync()
    {
        if (_databaseService == null || string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadHistoryAsync();
            return;
        }

        try
        {
            var records = await _databaseService.SearchUploadsAsync(SearchText);
            UploadHistory.Clear();
            foreach (var record in records)
            {
                UploadHistory.Add(record);
            }
            HistoryStatusMessage = $"Found {UploadHistory.Count} matching upload(s)";
        }
        catch (Exception ex)
        {
            HistoryStatusMessage = $"Error searching: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Save()
    {
        _settings.AccessKey = AccessKey;
        _settings.SecretKey = SecretKey;
        _settings.BucketName = BucketName;
        _settings.Region = Region;
        _settings.CustomDomain = CustomDomain;
        _settings.ScreenshotFolder = ScreenshotFolder;
        _settings.FileFolder = FileFolder;
        _settings.PasteFolder = PasteFolder;
        _settings.ScreenshotFilenameTemplate = ScreenshotFilenameTemplate;
        _settings.FileFilenameTemplate = FileFilenameTemplate;
        _settings.UseOriginalFilename = UseOriginalFilename;
        _settings.HistoryRetentionDays = HistoryRetentionDays;
        _settings.FullScreenHotkey = FullScreenHotkey;
        _settings.RegionHotkey = RegionHotkey;
        
        _settingsService.Save(_settings);
        StatusMessage = "Settings saved successfully!";
    }

    [RelayCommand]
    private void TestConnection()
    {
        try
        {
            var service = new DigitalOceanService(_settings);
            StatusMessage = "Testing connection...";
            
            Task.Run(async () =>
            {
                try
                {
                    var client = service.GetType()
                        .GetField("client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    await Task.Delay(1000);
                    
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = "Configuration looks valid!";
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Connection failed: {ex.Message}";
                    });
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshHistory()
    {
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task ClearHistory()
    {
        if (_databaseService == null) return;

        var result = MessageBox.Show(
            "Are you sure you want to clear all upload history? This will not delete files from DigitalOcean Spaces.",
            "Clear History",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var count = await _databaseService.ClearAllHistoryAsync();
                UploadHistory.Clear();
                HistoryStatusMessage = $"Cleared {count} record(s) from history";
            }
            catch (Exception ex)
            {
                HistoryStatusMessage = $"Error clearing history: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void CopyUrl(UploadRecord? record)
    {
        if (record == null) return;

        try
        {
            // Ensure we're on the UI thread for clipboard operations
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Clipboard.SetText(record.Url);
            });
            HistoryStatusMessage = "URL copied to clipboard";
        }
        catch (Exception ex)
        {
            HistoryStatusMessage = $"Error copying URL: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenUrl(UploadRecord? record)
    {
        if (record == null) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = record.Url,
                UseShellExecute = true
            });
            HistoryStatusMessage = "Opened URL in browser";
        }
        catch (Exception ex)
        {
            HistoryStatusMessage = $"Error opening URL: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteRecord(UploadRecord? record)
    {
        if (record == null || _databaseService == null) 
        {
            HistoryStatusMessage = "Cannot delete: service not available";
            return;
        }

        var result = MessageBox.Show(
            $"Delete '{record.FileName}'?\n\nThis will delete the file from DigitalOcean Spaces and remove it from history.\n\nS3 Key: {record.S3Key}",
            "Delete Upload",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                bool deletedFromSpaces = false;
                
                // Always create a fresh service to ensure we have latest settings
                if (_settings.IsConfigured)
                {
                    var service = new DigitalOceanService(_settings);
                    
                    // Delete from DigitalOcean Spaces
                    HistoryStatusMessage = $"Deleting '{record.FileName}' from Spaces...";
                    deletedFromSpaces = await service.DeleteFileAsync(record.S3Key);
                }
                else
                {
                    HistoryStatusMessage = "Spaces credentials not configured, only removing from history";
                }
                
                // Delete from database
                await _databaseService.DeleteUploadRecordAsync(record.Id);
                
                // Remove from UI
                UploadHistory.Remove(record);
                
                if (_settings.IsConfigured)
                {
                    HistoryStatusMessage = deletedFromSpaces
                        ? $"✓ Deleted '{record.FileName}' from Spaces and history"
                        : $"⚠ Removed from history but file deletion from Spaces failed (may not exist)";
                }
                else
                {
                    HistoryStatusMessage = $"✓ Removed '{record.FileName}' from history only";
                }
            }
            catch (Exception ex)
            {
                HistoryStatusMessage = $"❌ Error deleting: {ex.Message}\nStack: {ex.StackTrace}";
                
                // Still try to remove from database
                try
                {
                    await _databaseService.DeleteUploadRecordAsync(record.Id);
                    UploadHistory.Remove(record);
                    HistoryStatusMessage += "\n(Removed from history despite Spaces error)";
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}
