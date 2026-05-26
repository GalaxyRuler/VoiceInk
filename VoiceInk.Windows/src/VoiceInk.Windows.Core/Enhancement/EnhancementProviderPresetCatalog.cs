namespace VoiceInk.Windows.Core.Enhancement;

public static class EnhancementProviderPresetCatalog
{
    public static EnhancementProviderPreset Custom { get; } = new(
        "custom",
        "Custom OpenAI-compatible",
        string.Empty,
        string.Empty,
        [],
        RequiresApiKey: true);

    public static EnhancementProviderPreset Cerebras { get; } = new(
        "cerebras",
        "Cerebras",
        "https://api.cerebras.ai/v1/chat/completions",
        "gpt-oss-120b",
        [
            "gpt-oss-120b",
            "zai-glm-4.7"
        ],
        RequiresApiKey: true);

    public static EnhancementProviderPreset Groq { get; } = new(
        "groq",
        "Groq",
        "https://api.groq.com/openai/v1/chat/completions",
        "openai/gpt-oss-120b",
        [
            "llama-3.1-8b-instant",
            "llama-3.3-70b-versatile",
            "qwen/qwen3-32b",
            "openai/gpt-oss-120b",
            "openai/gpt-oss-20b"
        ],
        RequiresApiKey: true);

    public static EnhancementProviderPreset Gemini { get; } = new(
        "gemini",
        "Gemini",
        "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
        "gemini-2.5-flash-lite",
        [
            "gemini-3.5-flash",
            "gemini-3.1-pro-preview",
            "gemini-3-flash-preview",
            "gemini-3.1-flash-lite",
            "gemini-2.5-pro",
            "gemini-2.5-flash",
            "gemini-2.5-flash-lite"
        ],
        RequiresApiKey: true);

    public static EnhancementProviderPreset Anthropic { get; } = new(
        "anthropic",
        "Anthropic",
        "https://api.anthropic.com/v1/messages",
        "claude-sonnet-4-6",
        [
            "claude-opus-4-7",
            "claude-opus-4-6",
            "claude-sonnet-4-6",
            "claude-opus-4-5",
            "claude-sonnet-4-5",
            "claude-haiku-4-5"
        ],
        RequiresApiKey: true);

    public static EnhancementProviderPreset OpenAI { get; } = new(
        "openai",
        "OpenAI",
        "https://api.openai.com/v1/chat/completions",
        "gpt-5.4",
        [
            "gpt-5.5",
            "gpt-5.4",
            "gpt-5.4-mini",
            "gpt-5.4-nano",
            "gpt-5.2",
            "gpt-4.1",
            "gpt-4.1-mini",
            "gpt-4.1-nano"
        ],
        RequiresApiKey: true);

    public static EnhancementProviderPreset OpenRouter { get; } = new(
        "openrouter",
        "OpenRouter",
        "https://openrouter.ai/api/v1/chat/completions",
        "openai/gpt-oss-120b",
        [],
        RequiresApiKey: true);

    public static EnhancementProviderPreset Mistral { get; } = new(
        "mistral",
        "Mistral",
        "https://api.mistral.ai/v1/chat/completions",
        "mistral-large-latest",
        [
            "mistral-large-latest",
            "mistral-medium-latest",
            "mistral-small-latest"
        ],
        RequiresApiKey: true);

    public static EnhancementProviderPreset Ollama { get; } = new(
        "ollama",
        "Ollama",
        "http://localhost:11434/api/chat",
        "mistral",
        [],
        RequiresApiKey: false);

    public static EnhancementProviderPreset LocalCli { get; } = new(
        "local-cli",
        "Local CLI",
        string.Empty,
        "local-cli",
        [],
        RequiresApiKey: false);

    public static IReadOnlyList<EnhancementProviderPreset> All { get; } =
    [
        Custom,
        Cerebras,
        Groq,
        Gemini,
        Anthropic,
        OpenAI,
        OpenRouter,
        Mistral,
        Ollama,
        LocalCli
    ];

    public static EnhancementProviderPreset Resolve(string? id) =>
        All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? Custom;
}
