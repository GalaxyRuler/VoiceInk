# Windows Assistant Mode Guidance Spec

## Source of Truth

- VoiceInk AI Assistant Mode docs describe Assistant as a built-in Enhancement prompt.
- Users can select the Assistant prompt or use a trigger word for a single request.
- Assistant Mode answers the spoken request instead of merely formatting dictation.

## Windows Behavior

- Surface Assistant Mode in the Enhancement behavior rows.
- When Assistant is selected, show that conversational answer mode is active.
- Otherwise show that Assistant is available through the prompt picker or trigger words.
- Keep enhancement request construction, prompt rendering, trigger detection, and persistence unchanged.

## Non-Goals

- No new AI provider, web search, account flow, telemetry, or commercial assistant surface.
