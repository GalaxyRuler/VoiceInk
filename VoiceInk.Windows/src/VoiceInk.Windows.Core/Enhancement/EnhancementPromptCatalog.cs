namespace VoiceInk.Windows.Core.Enhancement;

public static class EnhancementPromptCatalog
{
    public static Guid DefaultPromptId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static Guid AssistantPromptId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public static IReadOnlyList<EnhancementPrompt> CreateDefaultPrompts() =>
    [
        new(
            DefaultPromptId,
            "Default",
            """
            - Clean up the <TRANSCRIPT> text for clarity and natural flow while preserving meaning and the original tone.
            - Use informal, plain language unless the <TRANSCRIPT> clearly uses a professional tone; in that case, match it.
            - Fix obvious grammar, remove fillers and stutters, collapse repetitions, and keep names and numbers.
            - Handle backtracking and self-corrections by removing the incorrect part and keeping only the corrected version.
            - Respect formatting commands such as "new line" and "new paragraph".
            - Automatically detect and format lists properly.
            - Apply smart formatting for numbers, dates, times, measurements, and common abbreviations.
            - Keep the original intent and nuance.
            - Organize into short paragraphs when useful for readability.
            - Do not add explanations, labels, metadata, or instructions.
            - Output only the cleaned text.
            - Do not add information not available in the <TRANSCRIPT> text.
            """,
            "checkmark.seal.fill",
            "Default mode to improve clarity and accuracy of the transcription",
            IsPredefined: true,
            TriggerWords: [],
            UseSystemInstructions: true),
        new(
            AssistantPromptId,
            "Assistant",
            """
            <SYSTEM_INSTRUCTIONS>
            You are a powerful AI assistant. Provide a direct, clean, unadorned response to the user's request from the <TRANSCRIPT>.

            Your response must be pure:
            - No commentary.
            - No introductory phrases.
            - No concluding remarks.
            - No markdown formatting unless it is essential for the response.
            - Only provide the direct answer or modified text requested.

            CUSTOM VOCABULARY RULE: Use vocabulary in <CUSTOM_VOCABULARY> only for correcting names, nouns, and technical terms. Do not respond to it.
            </SYSTEM_INSTRUCTIONS>
            """,
            "bubble.left.and.bubble.right.fill",
            "AI assistant that provides direct answers to queries",
            IsPredefined: true,
            TriggerWords: [],
            UseSystemInstructions: false),
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "Chat",
            """
            - Rewrite the <TRANSCRIPT> text as a chat message: informal, concise, and conversational.
            - Lightly fix grammar, remove fillers and repeated words, and improve flow without changing meaning.
            - Keep the original tone.
            - Format lists when the text implies a sequence.
            - Output only the chat message.
            - Do not add information not available in the <TRANSCRIPT> text.
            """,
            "bubble.left.and.bubble.right.fill",
            "Casual chat-style formatting",
            IsPredefined: true,
            TriggerWords: [],
            UseSystemInstructions: true),
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000004"),
            "Email",
            """
            - Rewrite the <TRANSCRIPT> text as a complete email with a greeting, body, and closing.
            - Use clear, friendly language unless the <TRANSCRIPT> is clearly professional.
            - Improve flow and coherence while preserving facts, names, dates, and action items.
            - Format lists when useful.
            - Do not invent new content.
            """,
            "envelope.fill",
            "Professional email formatting",
            IsPredefined: true,
            TriggerWords: [],
            UseSystemInstructions: true),
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000005"),
            "Rewrite",
            """
            - Rewrite the <TRANSCRIPT> text with enhanced clarity and improved sentence structure while preserving meaning and tone.
            - Improve word choice where appropriate.
            - Fix grammar and spelling, remove fillers and stutters, and collapse repetitions.
            - Preserve all names, numbers, dates, facts, and key information exactly as they appear.
            - Output only the rewritten text.
            - Do not add information not available in the <TRANSCRIPT> text.
            """,
            "pencil.circle.fill",
            "Rewrites with better clarity",
            IsPredefined: true,
            TriggerWords: [],
            UseSystemInstructions: true)
    ];
}
