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
            - Handle backtracking and self-corrections: When the speaker corrects themselves mid-sentence using phrases like "scratch that", "actually", "sorry not that", "I mean", "wait no", or similar corrections, remove the incorrect part and keep only the corrected version. Example: "The meeting is on Tuesday, sorry not that, actually Wednesday" -> "The meeting is on Wednesday."
            - Respect formatting commands: When the speaker explicitly says "new line" or "new paragraph", insert the appropriate line break or paragraph break at that point.
            - Automatically detect and format lists properly: if the <TRANSCRIPT> mentions a number, uses ordinal words, implies sequence or steps, or has a count before it, format as an ordered list; otherwise, format as an unordered list.
            - Apply smart formatting: Write numbers as numerals, convert common abbreviations to proper format, and format dates, times, and measurements consistently.
            - Keep the original intent and nuance.
            - Organize into short paragraphs of 2-4 sentences for readability.
            - Do not add explanations, labels, metadata, or instructions.
            - Output only the cleaned text.
            - Don't add any information not available in the <TRANSCRIPT> text ever.
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
            - Keep emotive markers and emojis if present; don't invent new ones.
            - Lightly fix grammar, remove fillers and repeated words, and improve flow without changing meaning.
            - Keep the original tone; only be professional if the <TRANSCRIPT> already is.
            - Automatically detect and format lists properly: if the <TRANSCRIPT> mentions a number, uses ordinal words, implies sequence or steps, or has a count before it, format as an ordered list; otherwise, format as an unordered list.
            - Write numbers as numerals.
            - Format like a modern chat message - short lines, natural breaks, emoji-friendly.
            - Do not add greetings, sign-offs, or commentary.
            - Output only the chat message.
            - Don't add any information not available in the <TRANSCRIPT> text ever.
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
            - Rewrite the <TRANSCRIPT> text as a complete email with proper formatting: include a greeting, body paragraphs, and closing.
            - Use clear, friendly, non-formal language unless the <TRANSCRIPT> is clearly professional; in that case, match that tone.
            - Improve flow and coherence; fix grammar and spelling; remove fillers; keep all facts, names, dates, and action items.
            - Automatically detect and format lists properly: if the <TRANSCRIPT> mentions a number, uses ordinal words, implies sequence or steps, or has a count before it, format as an ordered list; otherwise, format as an unordered list.
            - Write numbers as numerals.
            - Do not invent new content.
            - Don't add any information not available in the <TRANSCRIPT> text ever.
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
            - Rewrite the <TRANSCRIPT> text with enhanced clarity, improved sentence structure, and rhythmic flow while preserving the original meaning and tone.
            - Restructure sentences for better readability and natural progression.
            - Improve word choice and phrasing where appropriate, but maintain the original voice and intent.
            - Fix grammar and spelling, remove fillers and stutters, and collapse repetitions.
            - Format any lists as proper bullet points or numbered lists.
            - Write numbers as numerals.
            - Organize content into well-structured paragraphs of 2-4 sentences for optimal readability.
            - Preserve all names, numbers, dates, facts, and key information exactly as they appear.
            - Output only the rewritten text.
            - Don't add any information not available in the <TRANSCRIPT> text ever.
            """,
            "pencil.circle.fill",
            "Rewrites with better clarity",
            IsPredefined: true,
            TriggerWords: [],
            UseSystemInstructions: true)
    ];
}
