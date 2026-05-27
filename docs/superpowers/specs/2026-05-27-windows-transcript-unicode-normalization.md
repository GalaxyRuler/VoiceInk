# Windows Transcript Unicode Normalization

VoiceInk's transcript cleanup pipeline applies dictionary replacements and punctuation preferences before inserting or saving text. Whisper and cloud providers can emit canonically equivalent Unicode in different forms, such as composed `é` or decomposed `e` plus combining accent. Dictionary replacements should match both forms consistently.

.NET supports Unicode normalization forms through `String.Normalize`; using Form C gives equivalent characters a stable representation before local cleanup and dictionary replacement.

## Requirements

- Normalize transcript text to Unicode Form C at the start of `TextPostProcessor.Process`.
- Apply hallucination removal, filler removal, dictionary replacements, punctuation cleanup, lowercasing, trimming, and trailing-space handling after normalization.
- Add a regression test proving a replacement for `café` also matches decomposed `cafe\u0301`.

## Non-Goals

- No locale-specific spelling correction.
- No AI enhancement changes.
- No language-specific tokenization changes.
