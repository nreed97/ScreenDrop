using System;

namespace ScreenDrop.Models;

public class UploadRecord
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;  // For deletion
    public string Folder { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string UploadType { get; set; } = string.Empty;  // "Screenshot", "File", "Clipboard"
    public byte[]? ThumbnailData { get; set; }  // Small JPEG thumbnail
}
