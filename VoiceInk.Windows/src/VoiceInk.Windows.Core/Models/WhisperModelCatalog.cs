namespace VoiceInk.Windows.Core.Models;

public static class WhisperModelCatalog
{
    private static readonly WhisperModelCatalogEntry[] Models =
    [
        new(
            "ggml-tiny",
            "Tiny",
            "75 MB",
            IsMultilingual: true,
            "Tiny model, fastest, least accurate",
            Speed: 0.95,
            Accuracy: 0.6,
            RamUsage: 0.3),
        new(
            "ggml-tiny.en",
            "Tiny (English)",
            "75 MB",
            IsMultilingual: false,
            "Tiny model optimized for English, fastest, least accurate",
            Speed: 0.95,
            Accuracy: 0.65,
            RamUsage: 0.3),
        new(
            "ggml-base",
            "Base",
            "142 MB",
            IsMultilingual: true,
            "Base model, good balance between speed and accuracy, supports multiple languages",
            Speed: 0.85,
            Accuracy: 0.72,
            RamUsage: 0.5),
        new(
            "ggml-base.en",
            "Base (English)",
            "142 MB",
            IsMultilingual: false,
            "Base model optimized for English, good balance between speed and accuracy",
            Speed: 0.85,
            Accuracy: 0.75,
            RamUsage: 0.5),
        new(
            "ggml-large-v2",
            "Large v2",
            "2.9 GB",
            IsMultilingual: true,
            "Large model v2, slower than Medium but more accurate",
            Speed: 0.3,
            Accuracy: 0.96,
            RamUsage: 3.8),
        new(
            "ggml-large-v3",
            "Large v3",
            "2.9 GB",
            IsMultilingual: true,
            "Large model v3, very slow but most accurate",
            Speed: 0.3,
            Accuracy: 0.98,
            RamUsage: 3.9),
        new(
            "ggml-large-v3-turbo",
            "Large v3 Turbo",
            "1.5 GB",
            IsMultilingual: true,
            "Large model v3 Turbo, faster than v3 with similar accuracy",
            Speed: 0.75,
            Accuracy: 0.97,
            RamUsage: 1.8),
        new(
            "ggml-large-v3-turbo-q5_0",
            "Large v3 Turbo (Quantized)",
            "547 MB",
            IsMultilingual: true,
            "Quantized version of Large v3 Turbo, faster with slightly lower accuracy",
            Speed: 0.75,
            Accuracy: 0.95,
            RamUsage: 1.0)
    ];

    private static readonly string[] RecommendedModelNames =
    [
        "ggml-base.en",
        "ggml-large-v3-turbo-q5_0"
    ];

    public static IReadOnlyList<WhisperModelCatalogEntry> All { get; } = Models;

    public static IReadOnlyList<WhisperModelCatalogEntry> Recommended { get; } =
        RecommendedModelNames
            .Select(name => Models.Single(model => model.Name == name))
            .ToArray();
}
