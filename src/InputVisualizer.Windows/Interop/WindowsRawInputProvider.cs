using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using InputVisualizer.Core.Filter;
using InputVisualizer.Core.Input;

namespace InputVisualizer.Windows.Interop;

public sealed class WindowsRawInputProvider
{
    public const uint WmInput = 0x00FF;

    private const int RidInput = 0x10000003;
    private const int RimTypeMouse = 0;
    private const int RimTypeKeyboard = 1;
    private const int RidevInputSink = 0x00000100;
    private const ushort UsagePageGenericDesktop = 0x01;
    private const ushort UsageMouse = 0x02;
    private const ushort UsageKeyboard = 0x06;
    private const ushort RiKeyBreak = 0x0001;
    private const ushort MouseLeftButtonDown = 0x0001;
    private const ushort MouseLeftButtonUp = 0x0002;
    private const ushort MouseRightButtonDown = 0x0004;
    private const ushort MouseRightButtonUp = 0x0008;
    private const ushort MouseMiddleButtonDown = 0x0010;
    private const ushort MouseMiddleButtonUp = 0x0020;
    private const ushort MouseWheel = 0x0400;

    public void RegisterKeyboardAndMouse(IntPtr targetHwnd)
    {
        if (targetHwnd == IntPtr.Zero)
        {
            throw new ArgumentException("A valid window handle is required.", nameof(targetHwnd));
        }

        var devices = new[]
        {
            new RawInputDevice
            {
                UsagePage = UsagePageGenericDesktop,
                Usage = UsageKeyboard,
                Flags = RidevInputSink,
                Target = targetHwnd
            },
            new RawInputDevice
            {
                UsagePage = UsagePageGenericDesktop,
                Usage = UsageMouse,
                Flags = RidevInputSink,
                Target = targetHwnd
            }
        };

        if (!RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<RawInputDevice>()))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public IReadOnlyList<InputAction> ReadActionsFromMessage(IntPtr rawInputHandle)
    {
        if (rawInputHandle == IntPtr.Zero)
        {
            return [];
        }

        uint size = 0;
        _ = GetRawInputData(rawInputHandle, RidInput, IntPtr.Zero, ref size, (uint)Marshal.SizeOf<RawInputHeader>());
        if (size == 0)
        {
            return [];
        }

        var buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            var bytesRead = GetRawInputData(rawInputHandle, RidInput, buffer, ref size, (uint)Marshal.SizeOf<RawInputHeader>());
            if (bytesRead == uint.MaxValue || bytesRead != size)
            {
                return [];
            }

            var managed = new byte[size];
            Marshal.Copy(buffer, managed, 0, (int)size);
            return ParseRawInput(managed);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static IReadOnlyList<InputAction> ParseRawInput(ReadOnlySpan<byte> rawInput)
    {
        var dataOffset = 8 + (IntPtr.Size * 2);
        if (rawInput.Length < dataOffset + 16)
        {
            return [];
        }

        var type = BinaryPrimitives.ReadUInt32LittleEndian(rawInput[..4]);
        return type switch
        {
            RimTypeKeyboard => ParseKeyboard(rawInput[dataOffset..]),
            RimTypeMouse => ParseMouse(rawInput[dataOffset..]),
            _ => []
        };
    }

    private static IReadOnlyList<InputAction> ParseKeyboard(ReadOnlySpan<byte> keyboardData)
    {
        if (keyboardData.Length < 16)
        {
            return [];
        }

        var flags = BinaryPrimitives.ReadUInt16LittleEndian(keyboardData.Slice(2, 2));
        var virtualKey = BinaryPrimitives.ReadUInt16LittleEndian(keyboardData.Slice(6, 2));
        var id = ActionNormalizer.NormalizeKeyboardVirtualKey(virtualKey);
        if (id is null)
        {
            return [];
        }

        var pressed = (flags & RiKeyBreak) == 0;
        return [new InputAction(InputActionKind.Keyboard, id, pressed)];
    }

    private static IReadOnlyList<InputAction> ParseMouse(ReadOnlySpan<byte> mouseData)
    {
        if (mouseData.Length < 24)
        {
            return [];
        }

        var buttonFlags = BinaryPrimitives.ReadUInt16LittleEndian(mouseData.Slice(4, 2));
        var buttonData = unchecked((short)BinaryPrimitives.ReadUInt16LittleEndian(mouseData.Slice(6, 2)));
        var actions = new List<InputAction>(4);

        AddMouseButton(actions, buttonFlags, MouseLeftButtonDown, MouseLeftButtonUp, "Mouse1");
        AddMouseButton(actions, buttonFlags, MouseRightButtonDown, MouseRightButtonUp, "Mouse2");
        AddMouseButton(actions, buttonFlags, MouseMiddleButtonDown, MouseMiddleButtonUp, "Mouse3");

        if ((buttonFlags & MouseWheel) != 0 && buttonData != 0)
        {
            actions.Add(ActionNormalizer.MouseWheel(buttonData));
        }

        return actions;
    }

    private static void AddMouseButton(List<InputAction> actions, ushort flags, ushort downFlag, ushort upFlag, string id)
    {
        if ((flags & downFlag) != 0)
        {
            actions.Add(ActionNormalizer.MouseButton(id, true));
        }
        else if ((flags & upFlag) != 0)
        {
            actions.Add(ActionNormalizer.MouseButton(id, false));
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterRawInputDevices(
        RawInputDevice[] rawInputDevices,
        uint numberDevices,
        uint size);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputData(
        IntPtr rawInput,
        uint command,
        IntPtr data,
        ref uint size,
        uint sizeHeader);

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputDevice
    {
        public ushort UsagePage;
        public ushort Usage;
        public int Flags;
        public IntPtr Target;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputHeader
    {
        public uint Type;
        public uint Size;
        public IntPtr Device;
        public IntPtr WParam;
    }
}
