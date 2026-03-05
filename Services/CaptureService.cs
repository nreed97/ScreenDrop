using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ScreenDrop.Services;

public class CaptureService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height,
        IntPtr hdcSrc, int xSrc, int ySrc, int rop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    private const int SRCCOPY = 0x00CC0020;

    public byte[] CaptureFullScreen()
    {
        var screenWidth = (int)SystemParameters.VirtualScreenWidth;
        var screenHeight = (int)SystemParameters.VirtualScreenHeight;
        var screenLeft = (int)SystemParameters.VirtualScreenLeft;
        var screenTop = (int)SystemParameters.VirtualScreenTop;

        return CaptureRegion(screenLeft, screenTop, screenWidth, screenHeight);
    }

    public byte[] CaptureRegion(int x, int y, int width, int height)
    {
        IntPtr hDesk = GetDesktopWindow();
        IntPtr hSrce = GetWindowDC(hDesk);
        IntPtr hDest = CreateCompatibleDC(hSrce);
        IntPtr hBmp = CreateCompatibleBitmap(hSrce, width, height);
        IntPtr hOldBmp = SelectObject(hDest, hBmp);

        BitBlt(hDest, 0, 0, width, height, hSrce, x, y, SRCCOPY);

        SelectObject(hDest, hOldBmp);

        using var bmp = Image.FromHbitmap(hBmp);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        var bytes = ms.ToArray();

        DeleteObject(hBmp);
        DeleteDC(hDest);
        ReleaseDC(hDesk, hSrce);

        return bytes;
    }

    public BitmapSource? CaptureRegionAsBitmapSource(int x, int y, int width, int height)
    {
        var bytes = CaptureRegion(x, y, width, height);
        if (bytes.Length == 0) return null;

        using var ms = new MemoryStream(bytes);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = ms;
        bitmap.EndInit();
        bitmap.Freeze();
        
        return bitmap;
    }

    public BitmapSource? CaptureFullScreenAsBitmapSource()
    {
        var screenWidth = (int)SystemParameters.VirtualScreenWidth;
        var screenHeight = (int)SystemParameters.VirtualScreenHeight;
        var screenLeft = (int)SystemParameters.VirtualScreenLeft;
        var screenTop = (int)SystemParameters.VirtualScreenTop;

        return CaptureRegionAsBitmapSource(screenLeft, screenTop, screenWidth, screenHeight);
    }
}
