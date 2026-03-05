using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ScreenDrop.Helpers;

public class GlobalHotkey : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly int _id;
    private readonly IntPtr _hwnd;
    private readonly HwndSource _source;
    private bool _disposed;

    public GlobalHotkey(Window window, int id, HotkeyModifiers modifiers, Key key)
    {
        _id = id;
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.Handle;
        
        _source = HwndSource.FromHwnd(_hwnd);
        _source.AddHook(HwndHook);
        
        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        var mod = (uint)modifiers;
        
        if (!RegisterHotKey(_hwnd, id, mod, vk))
        {
            throw new InvalidOperationException("Failed to register hotkey");
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    public event EventHandler? HotkeyPressed;

    public void Dispose()
    {
        if (_disposed) return;
        
        _source.RemoveHook(HwndHook);
        UnregisterHotKey(_hwnd, _id);
        _disposed = true;
    }
}

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}
