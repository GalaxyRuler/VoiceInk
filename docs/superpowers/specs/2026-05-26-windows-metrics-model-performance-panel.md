# Windows Metrics Model Performance Panel Spec

Date: 2026-05-26

## Goal

Make Metrics model performance rows easier to scan by exposing presenter-backed value/detail/status fields and rendering them with row templates instead of plain strings.

## Source Of Truth

VoiceInk troubleshooting and model guidance centers on transcription speed and model choice. The Windows Metrics page already records local session metrics; this slice improves presentation only.

## Windows Behavior

- Extend `ModelPerformanceRow` with a primary value and status badge.
- Transcription rows show speed factor as the primary value and sessions/duration as supporting details.
- Enhancement rows show average processing latency as the primary value and sessions as supporting details.
- Empty rows remain explicit and non-commercial.
- WinUI renders templated rows for transcription and enhancement model lists.
- Do not change metrics persistence, aggregation, filtering, CSV export, or diagnostics behavior.

