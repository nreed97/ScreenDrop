using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenDrop.Converters;

public class ByteArrayToImageConverter : IValueConverter
{
    private static BitmapSource? _placeholderImage;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] bytes || bytes.Length == 0)
            return GetPlaceholderImage();

        try
        {
            using var ms = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return GetPlaceholderImage();
        }
    }

    private static BitmapSource GetPlaceholderImage()
    {
        if (_placeholderImage == null)
        {
            // Create a simple gray placeholder image
            var width = 60;
            var height = 60;
            var dpi = 96;
            var pixelFormat = PixelFormats.Bgr24;
            var stride = width * ((pixelFormat.BitsPerPixel + 7) / 8);
            var pixels = new byte[height * stride];
            
            // Fill with gray color
            for (int i = 0; i < pixels.Length; i += 3)
            {
                pixels[i] = 200;     // B
                pixels[i + 1] = 200; // G
                pixels[i + 2] = 200; // R
            }
            
            _placeholderImage = BitmapSource.Create(width, height, dpi, dpi, pixelFormat, null, pixels, stride);
            _placeholderImage.Freeze();
        }
        
        return _placeholderImage;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
