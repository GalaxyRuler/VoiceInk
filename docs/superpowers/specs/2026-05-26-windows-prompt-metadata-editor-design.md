# Windows Prompt Metadata Editor Design

## Goal

Expose custom prompt icon and description editing in the Windows Enhancement page so custom prompts preserve the same metadata shape as the macOS app.

## Grounding

- The Core `EnhancementPrompt` model already stores `Icon` and `Description`.
- `EnhancementPromptLibrary.CreateCustomPrompt` already normalizes empty icons to `doc.text.fill` and trims descriptions.
- Microsoft's WinUI TextBox documentation supports standard single-line text fields with headers and placeholder text, which fits these compact metadata fields.

## Behavior

- Add `Prompt icon` and `Prompt description` fields to the Enhancement prompt editor.
- Populate the fields from the selected prompt.
- For new custom prompts, default the icon to `doc.text.fill` and clear the description.
- Save custom prompt icon and description through the existing prompt persistence path.
- Keep predefined prompt title, instructions, icon, description, and system-instruction mode read-only; predefined trigger words remain editable as before.

## Verification

- Build Debug x64 to verify XAML names and code-behind wiring.
- Run full solution tests and `git diff --check`.
