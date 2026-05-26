# Windows Transcribe Audio Drag And Drop Design

## Goal

Let users drop supported audio/video files directly onto the Transcribe Audio page, matching the original app's fluid file-import workflow while reusing the existing Windows queue pipeline.

## Grounding

- Microsoft WinUI drag-and-drop docs describe accepting `StorageItems` from a drop data package and reading dropped files with `GetStorageItemsAsync`.
- The Windows app already has picker-based Transcribe Audio queueing and Core validation for supported extensions, missing files, and duplicate active files.

## Behavior

- The Transcribe Audio page accepts file drops when the queue is editable.
- Dropped `StorageFile` items are converted to local paths and passed through `AudioFileQueueService.AddFiles`.
- Unsupported, missing, duplicate active, folder, and virtual/no-path items are skipped by the existing queue validation.
- Status text reports how many dropped files were queued and how many were skipped.

## Verification

- Debug x64 build because the change touches WinUI XAML events.
- Full tests before commit.
