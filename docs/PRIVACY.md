# Privacy

InputVisualizer only uses transient local state required to draw the current overlay frame.

Handled locally:

- The current foreground process name.
- Current pressed state for allowed action keys.
- Current mouse button and wheel actions.
- Current gamepad button, trigger, and stick actions.
- Overlay hidden reason.

Not collected:

- Typed text.
- Passwords.
- Chat messages.
- Clipboard text.
- Browser history.
- Audio, video, account, or telemetry data.

The default configuration disables the overlay at startup and requires explicit resume after risk states.
