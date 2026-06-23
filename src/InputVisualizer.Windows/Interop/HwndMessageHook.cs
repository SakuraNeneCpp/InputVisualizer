using System.ComponentModel;
using System.Runtime.InteropServices;

namespace InputVisualizer.Windows.Interop;

public sealed class HwndMessageHook : IDisposable
{
    private const int GwlpWndProc = -4;

    private readonly IntPtr _hwnd;
    private readonly WndProc _wndProc;
    private IntPtr _previousWndProc;
    private bool _disposed;

    private HwndMessageHook(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            throw new ArgumentException("A valid window handle is required.", nameof(hwnd));
        }

        _hwnd = hwnd;
        _wndProc = WindowProcedure;
        _previousWndProc = SetWindowLongPtr(_hwnd, GwlpWndProc, Marshal.GetFunctionPointerForDelegate(_wndProc));
        if (_previousWndProc == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
        }
    }

    public event EventHandler<WindowMessageEventArgs>? MessageReceived;

    public static HwndMessageHook Attach(IntPtr hwnd)
    {
        return new HwndMessageHook(hwnd);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_previousWndProc != IntPtr.Zero)
        {
            _ = SetWindowLongPtr(_hwnd, GwlpWndProc, _previousWndProc);
            _previousWndProc = IntPtr.Zero;
        }

        _disposed = true;
    }

    private IntPtr WindowProcedure(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        MessageReceived?.Invoke(this, new WindowMessageEventArgs(hwnd, message, wParam, lParam));
        return CallWindowProc(_previousWndProc, hwnd, message, wParam, lParam);
    }

    private static IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr newLong)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLongPtr64(hwnd, index, newLong);
        }

        return new IntPtr(SetWindowLong32(hwnd, index, newLong.ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private delegate IntPtr WndProc(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
}

public sealed class WindowMessageEventArgs(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam) : EventArgs
{
    public IntPtr Hwnd { get; } = hwnd;

    public uint Message { get; } = message;

    public IntPtr WParam { get; } = wParam;

    public IntPtr LParam { get; } = lParam;
}
