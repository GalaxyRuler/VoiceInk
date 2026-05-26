# Windows Speechmatics And Cartesia Provider Probes Plan

## Goal

Add safe, metadata-only provider test requests for the remaining Speechmatics and Cartesia transcription presets so the Windows fork can validate stored API keys without uploading audio, creating jobs, or leaking response bodies.

## Grounding

- Speechmatics Batch SaaS/V2 Jobs API supports Bearer-authenticated Jobs API requests under `https://...asr.api.speechmatics.com/v2/jobs`.
- Cartesia API conventions require HTTPS, `Authorization: Bearer <token>`, and `Cartesia-Version`; the datasets list endpoint is a small metadata request.

## Steps

- [x] Add failing infrastructure tests for Speechmatics and Cartesia probe request construction.
- [x] Route Speechmatics `Test Provider` through `GET https://eu1.asr.api.speechmatics.com/v2/jobs?limit=1` with Bearer auth.
- [x] Route Cartesia `Test Provider` through `GET https://api.cartesia.ai/datasets/?limit=1` with Bearer auth and `Cartesia-Version: 2026-03-01`.
- [x] Preserve missing-key short-circuit behavior and sanitized HTTP error messages.
- [x] Run focused infrastructure tests.
- [x] Run full solution tests and Debug x64 build.
- [x] Run whitespace diff check.
- [x] Update README/completion tracker.
- [ ] Commit the slice.

## Verification

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~ProbeAsync_SpeechmaticsUsesJobsEndpointAndBearerHeader|FullyQualifiedName~ProbeAsync_CartesiaUsesSafeMetadataEndpointAndVersionedBearerHeader|FullyQualifiedName~ProbeAsync_CustomProviderWithUnsupportedEndpointReturnsClearMessageWithoutHttp" -v minimal
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
