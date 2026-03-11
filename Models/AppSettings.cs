using System;
using System.Text.Json.Serialization;

namespace ScreenDrop.Models;

public class AppSettings
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "nyc3";
    public string CustomDomain { get; set; } = string.Empty;
    
    // Upload Folders
    public string ScreenshotFolder { get; set; } = "caps";
    public string FileFolder { get; set; } = "files";
    public string PasteFolder { get; set; } = "pastes";
    
    // Filename Templates
    public string ScreenshotFilenameTemplate { get; set; } = "{timestamp}_{random}";
    public string FileFilenameTemplate { get; set; } = "{timestamp}_{random}";
    public bool UseOriginalFilename { get; set; } = false;
    
    // History Management
    public int HistoryRetentionDays { get; set; } = 30; // 0 = keep forever
    
    // Keybindings
    public string FullScreenHotkey { get; set; } = "Ctrl+Shift+S";
    public string RegionHotkey { get; set; } = "Ctrl+Shift+R";
    
    [JsonIgnore]
    public string Endpoint => $"{BucketName}.{Region}.digitaloceanspaces.com";

    [JsonIgnore]
    public bool IsConfigured => !string.IsNullOrEmpty(AccessKey) && 
                                 !string.IsNullOrEmpty(SecretKey) && 
                                 !string.IsNullOrEmpty(BucketName);

    public string GetPublicUrl(string fileName)
    {
        // URL encode the path parts to handle spaces and special characters
        var parts = fileName.Split('/');
        var encodedParts = new string[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            encodedParts[i] = Uri.EscapeDataString(parts[i]);
        }
        var encodedFileName = string.Join("/", encodedParts);
        
        if (!string.IsNullOrEmpty(CustomDomain))
        {
            return $"https://{CustomDomain}/{encodedFileName}";
        }
        return $"https://{Endpoint}/{encodedFileName}";
    }
}
