using System.Runtime.InteropServices;
using InputVisualizer.Core.Input;

namespace InputVisualizer.Windows;

public sealed class XInputGamepadProvider : IInputProvider
{
    private const uint ErrorSuccess = 0;
    private const byte TriggerThreshold = 30;
    private const short LeftStickDeadZone = 7849;
    private const short RightStickDeadZone = 8689;

    public InputSnapshot ReadCurrent()
    {
        var actions = new List<InputAction>();

        for (uint index = 0; index < 4; index++)
        {
            if (XInputGetState(index, out var state) != ErrorSuccess)
            {
                continue;
            }

            AddButton(actions, state.Gamepad.Buttons, 0x1000, "A");
            AddButton(actions, state.Gamepad.Buttons, 0x2000, "B");
            AddButton(actions, state.Gamepad.Buttons, 0x4000, "X");
            AddButton(actions, state.Gamepad.Buttons, 0x8000, "Y");
            AddButton(actions, state.Gamepad.Buttons, 0x0100, "LB");
            AddButton(actions, state.Gamepad.Buttons, 0x0200, "RB");

            if (state.Gamepad.LeftTrigger > TriggerThreshold)
            {
                actions.Add(new InputAction(InputActionKind.GamepadButton, "LT", true, state.Gamepad.LeftTrigger / 255.0));
            }

            if (state.Gamepad.RightTrigger > TriggerThreshold)
            {
                actions.Add(new InputAction(InputActionKind.GamepadButton, "RT", true, state.Gamepad.RightTrigger / 255.0));
            }

            if (Math.Abs(state.Gamepad.ThumbLX) > LeftStickDeadZone ||
                Math.Abs(state.Gamepad.ThumbLY) > LeftStickDeadZone)
            {
                actions.Add(new InputAction(InputActionKind.GamepadAxis, "LeftStick", true));
            }

            if (Math.Abs(state.Gamepad.ThumbRX) > RightStickDeadZone ||
                Math.Abs(state.Gamepad.ThumbRY) > RightStickDeadZone)
            {
                actions.Add(new InputAction(InputActionKind.GamepadAxis, "RightStick", true));
            }
        }

        return new InputSnapshot(actions);
    }

    private static void AddButton(List<InputAction> actions, ushort buttons, ushort mask, string id)
    {
        if ((buttons & mask) != 0)
        {
            actions.Add(new InputAction(InputActionKind.GamepadButton, id, true));
        }
    }

    [DllImport("xinput1_4.dll")]
    private static extern uint XInputGetState(uint userIndex, out XInputState state);

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputState
    {
        public uint PacketNumber;
        public XInputGamepad Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputGamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX;
        public short ThumbLY;
        public short ThumbRX;
        public short ThumbRY;
    }
}
