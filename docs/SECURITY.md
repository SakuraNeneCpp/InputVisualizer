# Security

InputVisualizer is designed to fail closed. It only renders input when the foreground process is explicitly whitelisted, text input mode is not active, panic mode is not active, and the safety gate can make a positive allow decision.

Security invariants:

- Input history is not stored.
- Raw key sequences are not logged.
- Unknown foreground windows are hidden by default.
- Text input mode hides all actions.
- Panic mode clears current actions and keeps the overlay hidden until reset.
- Risk states require manual resume by default.
- Network transmission is not part of the application.

Do not add diagnostic logging that includes key names, input order, clipboard contents, text strings, or crash context that can reconstruct user input.
