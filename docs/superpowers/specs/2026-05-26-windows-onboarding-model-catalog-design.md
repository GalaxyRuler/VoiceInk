# Windows Onboarding Model Catalog Design

## Goal

Let first-run users choose and download a recommended local Whisper GGML model from onboarding instead of requiring a manually typed path.

## Source Grounding

The upstream whisper.cpp documentation and Hugging Face model page list pre-converted GGML `.bin` models, including `base.en`, and the Windows app already has a free/open-source local model downloader against those artifacts. This slice reuses the existing catalog/downloader and does not add commercial model channels.

## Behavior

- Onboarding shows a recommended local model selector backed by `WhisperModelCatalog.Recommended`.
- The default onboarding recommendation is `ggml-base.en`.
- A `Download Recommended Model` action downloads the selected catalog model into the existing app-local models directory.
- A successful onboarding download fills the model path field and app model path field, refreshes model choices, and leaves final setup completion to the existing `Save Setup` action.
- Download failures and cancellation stay local status messages.

## Non-Goals

- No new model provider.
- No paid model source.
- No automatic download without user action.
- No checksum verification in this slice.

## Verification

- Core tests cover recommended choices and default recommendation.
- App build verifies first-run dialog wiring.
- Full solution tests and Debug x64 build pass before commit.
