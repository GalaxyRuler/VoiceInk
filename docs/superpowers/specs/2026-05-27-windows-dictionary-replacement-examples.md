# Windows Dictionary Replacement Examples Spec

Date: 2026-05-27

## Goal

Bring the Windows Dictionary page closer to the macOS Word Replacement guidance by showing explicit alias and example guidance in the existing rule guidance surface.

## Source Of Truth

The macOS `WordReplacementInfoPopover` explains that multiple originals can be separated with commas and shows examples for links and product-name corrections. Windows already supports comma-separated originals and replacement cleanup; the gap is discoverability.

## Windows Behavior

- Add presenter-backed guidance for comma-separated replacement aliases.
- Add presenter-backed examples for link phrases and product-name corrections.
- Render through the existing Dictionary rule guidance list without adding a new modal or changing storage.
- Keep all dictionary import/export, quick add, sorting, replacement matching, and cleanup behavior unchanged.

## Non-Goals

- Do not change replacement matching semantics.
- Do not add cloud sync, accounts, telemetry, or commercial dictionary templates.
- Do not introduce a separate help popover until the broader Dictionary layout is revisited.
