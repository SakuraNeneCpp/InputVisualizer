using System.Diagnostics;
using System.Runtime.InteropServices;
using InputVisualizer.Core.Safety;

namespace InputVisualizer.Windows;

public sealed class ForegroundWindowGuard : IWindowSafetyProbe
{
    public ForegroundWindowInfo GetForegroundWindow()
    {
        var hwnd = GetForegroundWindowNative();
        if (hwnd == IntPtr.Zero)
        {
            return new ForegroundWindowInfo(false, null);
        }

        _ = GetWindowThreadProcessId(hwnd, out var processId);
        if (processId == 0)
        {
            return new ForegroundWindowInfo(true, null, IsIconic(hwnd));
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            var processName = process.ProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? process.ProcessName
                : $"{process.ProcessName}.exe";

            return new ForegroundWindowInfo(true, processName, IsIconic(hwnd));
        }
        catch (InvalidOperationException)
        {
            return new ForegroundWindowInfo(true, null, IsIconic(hwnd));
        }
        catch (ArgumentException)
        {
            return new ForegroundWindowInfo(true, null, IsIconic(hwnd));
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return new ForegroundWindowInfo(true, null, IsIconic(hwnd));
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    private static extern IntPtr GetForegroundWindowNative();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hWnd);
}
