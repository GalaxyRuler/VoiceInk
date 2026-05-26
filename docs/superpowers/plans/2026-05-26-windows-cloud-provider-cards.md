# Windows Cloud Provider Cards Implementation Plan

**Goal:** Add macOS-style metadata to the Windows cloud transcription provider selector and surface it in the AI Models section.

**Tech Stack:** .NET 10, WinUI 3, xUnit, existing `TranscriptionProviderPresetCatalog`.

## Task 1: Add Catalog Metadata Tests

Add focused tests requiring every cloud preset to expose description, language, speed, accuracy, and streaming/batch labels.

Status: completed.

## Task 2: Extend Provider Presets

Extend `TranscriptionProviderPreset` and `TranscriptionProviderPresetCatalog` with provider-card metadata aligned to the macOS cloud model descriptions and Windows implemented capabilities.

Status: completed.

## Task 3: Surface Metadata In AI Models

Add a compact WinUI provider details panel bound to the selected cloud transcription preset, keeping the existing endpoint/model/key controls available.

Status: completed.

## Task 4: Document And Verify

Update README and the completion tracker, then run focused tests, full solution tests, Debug x64 build, and `git diff --check`.

Status: completed.
