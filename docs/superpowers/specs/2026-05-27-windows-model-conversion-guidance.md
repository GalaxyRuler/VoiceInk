# Windows Model Conversion Guidance

## Goal

Close a local model lifecycle gap by making the Windows AI Models overview explicit about custom and fine-tuned Whisper model requirements.

## Source Of Truth

- VoiceInk docs say custom local models must be whisper.cpp-compatible `.bin` files.
- The macOS AI Models flow lets users import local models and set them as defaults from the model card.
- whisper.cpp documents pre-converted GGML downloads and conversion paths for PyTorch or Hugging Face checkpoints.

## Requirements

- Add a presenter-backed Model Library guidance row for custom/fine-tuned model conversion.
- The row must say that imported custom models must already be whisper.cpp-compatible `.bin` files.
- The row must tell users to convert PyTorch or Hugging Face checkpoints before importing.
- Keep this behavior in Core presenter logic so WinUI remains a binding surface.
- Do not add commercial links, paid gates, telemetry, or account flows.
