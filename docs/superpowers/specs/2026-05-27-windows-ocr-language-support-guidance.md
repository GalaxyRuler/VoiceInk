# Windows OCR Language Support Guidance Spec

Date: 2026-05-27

## Goal

Improve Context Awareness diagnostics by explaining that Windows OCR context depends on local Windows OCR recognizers and language support.

## Source Of Truth

Microsoft's Windows OCR documentation describes OCR as a local Windows recognizer API with available recognizer languages. VoiceInk already degrades gracefully when OCR produces no text; users need visible guidance that language support can affect OCR output.

## Windows Behavior

- When Screen OCR context is enabled, the Enhancement context privacy rows include an `OCR Language Support` row.
- The row explains that local Windows OCR recognizers are used and unsupported languages may yield empty or partial context.
- The guidance is local-only and does not imply cloud OCR, model downloads, telemetry, or automatic language installation.
- OCR capture, prompt rendering, provider selection, fallback, and history behavior remain unchanged.

## Non-Goals

- Do not install or modify Windows language packs.
- Do not add a new OCR engine or cloud OCR provider.
- Do not change the existing graceful empty-OCR prompt behavior.
