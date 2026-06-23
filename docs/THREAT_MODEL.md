# Threat Model

## Assets

- Passwords and authentication input.
- Personal text typed into chat, browser, terminal, email, or game text fields.
- Clipboard contents.
- Stream output shown through OBS or recordings.

## Primary Threats

- A user alt-tabs from a game to a text field while input remains visible.
- A login or launcher window receives keyboard focus.
- Game chat mode turns movement keys into private text.
- Debug logs or crash dumps preserve input order.
- A panic state automatically resumes without user intent.

## Mitigations

- Whitelist-only foreground process checks.
- Fail-closed handling for unknown foreground windows.
- Text input guard triggered by common chat open keys and manual controls.
- Panic mode that clears current input and requires reset.
- Allowed action filter that suppresses unapproved keys and text-control keys.
- No input history and no key-name logging API.

## Residual Risk

UI Automation password detection is only a secondary defense. Some games, launchers, browser forms, and custom UIs may not expose password metadata. The primary safety boundary is still the process whitelist plus text-input hiding.
