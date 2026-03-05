using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ScreenDrop.Models;

namespace ScreenDrop.Services;

public class DigitalOceanService
{
    private readonly AppSettings _settings;
    private AmazonS3Client? _client;

    public DigitalOceanService(AppSettings settings)
    {
        _settings = settings;
    }

    private AmazonS3Client GetClient()
    {
        if (_client == null)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{_settings.Region}.digitaloceanspaces.com",
                ForcePathStyle = true
            };
            
            _client = new AmazonS3Client(
                _settings.AccessKey,
                _settings.SecretKey,
                config);
        }
        return _client;
    }

    public async Task<UploadResult> UploadImageAsync(byte[] imageData, string extension, string folder = "caps")
    {
        var template = _settings.ScreenshotFilenameTemplate;
        var baseFileName = FilenameTemplateService.Generate(template, "screenshot");
        var fileName = $"{baseFileName}.{extension}";
        var key = string.IsNullOrWhiteSpace(folder) ? fileName : $"{folder.Trim('/')}/{fileName}";
        
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = new MemoryStream(imageData),
            ContentType = GetContentType(extension),
            CannedACL = S3CannedACL.PublicRead
        };

        var client = GetClient();
        await client.PutObjectAsync(request);

        return new UploadResult
        {
            Url = _settings.GetPublicUrl(key),
            S3Key = key,
            FileName = fileName
        };
    }

    public async Task<UploadResult> UploadFileAsync(string filePath, string folder = "files", bool useOriginalFilename = false)
    {
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLower();
        string fileName;
        
        if (useOriginalFilename)
        {
            // Use original filename
            fileName = Path.GetFileName(filePath);
        }
        else
        {
            // Use template
            var template = _settings.FileFilenameTemplate;
            var baseFileName = FilenameTemplateService.Generate(template, "file");
            fileName = $"{baseFileName}.{extension}";
        }
        
        var key = string.IsNullOrWhiteSpace(folder) ? fileName : $"{folder.Trim('/')}/{fileName}";
        
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            FilePath = filePath,
            ContentType = GetContentType(extension),
            CannedACL = S3CannedACL.PublicRead
        };

        var client = GetClient();
        await client.PutObjectAsync(request);

        return new UploadResult
        {
            Url = _settings.GetPublicUrl(key),
            S3Key = key,
            FileName = fileName
        };
    }

    public async Task<bool> DeleteFileAsync(string s3Key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = s3Key
            };

            var client = GetClient();
            var response = await client.DeleteObjectAsync(request);
            
            // S3 DeleteObject always returns 204 No Content even if object doesn't exist
            // So we check the HTTP status code
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent ||
                   response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            // Log the error but return false
            System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}");
            throw; // Re-throw so caller can see the actual error
        }
    }

    public static byte[]? GenerateThumbnail(byte[] imageData, int maxSize = 200)
    {
        try
        {
            using var ms = new MemoryStream(imageData);
            using var originalImage = Image.FromStream(ms);

            // Calculate new size maintaining aspect ratio
            int newWidth, newHeight;
            if (originalImage.Width > originalImage.Height)
            {
                newWidth = maxSize;
                newHeight = (int)((double)originalImage.Height / originalImage.Width * maxSize);
            }
            else
            {
                newHeight = maxSize;
                newWidth = (int)((double)originalImage.Width / originalImage.Height * maxSize);
            }

            using var thumbnail = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(thumbnail);
            
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);

            using var thumbnailMs = new MemoryStream();
            thumbnail.Save(thumbnailMs, ImageFormat.Jpeg);
            return thumbnailMs.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static string GetContentType(string extension)
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
            _ => "application/octet-stream"
        };
    }
}
