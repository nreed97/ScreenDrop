using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ScreenDrop.Services;

public class ClipboardService
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private HwndSource? _hwndSource;
    private IntPtr _hwnd;
    private bool _isMonitoring;
    private readonly Action<byte[]> _onImageCaptured;

    public ClipboardService(Action<byte[]> onImageCaptured)
    {
        _onImageCaptured = onImageCaptured;
    }

    public void StartMonitoring(Window window)
    {
        if (_isMonitoring) return;

        var helper = new WindowInteropHelper(window);
        _hwnd = helper.Handle;
        
        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);
        
        AddClipboardFormatListener(_hwnd);
        _isMonitoring = true;
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring) return;

        RemoveClipboardFormatListener(_hwnd);
        _hwndSource?.RemoveHook(WndProc);
        _isMonitoring = false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            ProcessClipboard();
        }
        return IntPtr.Zero;
    }

    private void ProcessClipboard()
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        var bytes = BitmapSourceToBytes(image);
                        if (bytes.Length > 0)
                        {
                            _onImageCaptured(bytes);
                        }
                    }
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList();
                    if (files.Count > 0)
                    {
                        var filePath = files[0];
                        if (filePath != null && IsImageFile(filePath) && File.Exists(filePath))
                        {
                            var bytes = File.ReadAllBytes(filePath);
                            _onImageCaptured(bytes);
                        }
                    }
                }
            });
        }
        catch
        {
        }
    }

    private static byte[] BitmapSourceToBytes(BitmapSource bitmapSource)
    {
        using var stream = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp";
    }
}
