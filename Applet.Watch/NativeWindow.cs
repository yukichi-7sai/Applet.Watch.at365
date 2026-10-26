using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Applets.Watch;

internal static class NativeWindow
{
    public const int ExStyleIndex = -20;
    public const int OverlayStyles = 0x00000020 | 0x00000080 | 0x08000000; // transparent / toolwindow / noactivate
    private static readonly nint Topmost = new(-1);
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)] public static extern int GetWindowLong(nint hwnd, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)] private static extern int SetWindowLong(nint hwnd, int index, int value);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetWindowRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool IsWindowVisible(nint hwnd);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(nint hwnd);
    public static void Overlay(nint hwnd)
    {
        SetWindowLong(hwnd, ExStyleIndex, GetWindowLong(hwnd, ExStyleIndex) | OverlayStyles);
        if (!SetWindowPos(hwnd, Topmost, 0, 0, 0, 0, 0x0010 | 0x0001 | 0x0002 | 0x0020)) throw new Win32Exception();
    }
    public static void Move(nint hwnd, PixelBounds bounds)
    {
        if (!SetWindowPos(hwnd, Topmost, bounds.Left, bounds.Top, bounds.Width, bounds.Height, 0x0010 | 0x0200)) throw new Win32Exception();
    }
}
