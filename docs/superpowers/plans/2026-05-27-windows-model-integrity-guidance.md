# Windows Model Integrity Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add checksum/provenance guidance to the Local Whisper Library overview.

---

## Task 1: Red Tests

- [x] Add Model Library overview expectations for an Integrity Check storage guidance row.
- [x] Confirm focused tests fail before implementation.

## Task 2: Presenter Guidance

- [x] Add the Integrity Check row to `ModelLibraryOverviewPresenter`.
- [x] Run the focused model tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: upstream whisper.cpp documents GGML `.bin` model files and public Hugging Face model downloads; checksum verification remains user/source dependent.
- Red: focused Model Library tests failed because the Integrity Check row was absent.
- Green: focused Model Library tests passed after adding the row.
- Full test: solution test passed with 777 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
