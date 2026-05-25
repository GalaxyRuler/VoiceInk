namespace VoiceInk.Windows.Core.Models;

public static class WhisperLanguageCatalog
{
    private static readonly string[] WhisperLanguageCodes =
    [
        "auto",
        "af", "am", "ar", "as", "az", "ba", "be", "bg", "bn", "bo",
        "br", "bs", "ca", "cs", "cy", "da", "de", "el", "en", "es",
        "et", "eu", "fa", "fi", "fo", "fr", "gl", "gu", "ha", "haw",
        "he", "hi", "hr", "ht", "hu", "hy", "id", "is", "it", "ja",
        "jw", "ka", "kk", "km", "kn", "ko", "la", "lb", "ln", "lo",
        "lt", "lv", "mg", "mi", "mk", "ml", "mn", "mr", "ms", "mt",
        "my", "ne", "nl", "nn", "no", "oc", "pa", "pl", "ps", "pt",
        "ro", "ru", "sa", "sd", "si", "sk", "sl", "sn", "so", "sq",
        "sr", "su", "sv", "sw", "ta", "te", "tg", "th", "tk", "tl",
        "tr", "tt", "uk", "ur", "uz", "vi", "yi", "yo", "yue", "zh"
    ];

    private static readonly IReadOnlyDictionary<string, string> LanguageNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["auto"] = "Auto-detect",
            ["af"] = "Afrikaans",
            ["am"] = "Amharic",
            ["ar"] = "Arabic",
            ["as"] = "Assamese",
            ["az"] = "Azerbaijani",
            ["ba"] = "Bashkir",
            ["be"] = "Belarusian",
            ["bg"] = "Bulgarian",
            ["bn"] = "Bengali",
            ["bo"] = "Tibetan",
            ["br"] = "Breton",
            ["bs"] = "Bosnian",
            ["ca"] = "Catalan",
            ["cs"] = "Czech",
            ["cy"] = "Welsh",
            ["da"] = "Danish",
            ["de"] = "German",
            ["el"] = "Greek",
            ["en"] = "English",
            ["es"] = "Spanish",
            ["et"] = "Estonian",
            ["eu"] = "Basque",
            ["fa"] = "Persian",
            ["fi"] = "Finnish",
            ["fo"] = "Faroese",
            ["fr"] = "French",
            ["gl"] = "Galician",
            ["gu"] = "Gujarati",
            ["ha"] = "Hausa",
            ["haw"] = "Hawaiian",
            ["he"] = "Hebrew",
            ["hi"] = "Hindi",
            ["hr"] = "Croatian",
            ["ht"] = "Haitian Creole",
            ["hu"] = "Hungarian",
            ["hy"] = "Armenian",
            ["id"] = "Indonesian",
            ["is"] = "Icelandic",
            ["it"] = "Italian",
            ["ja"] = "Japanese",
            ["jw"] = "Javanese",
            ["ka"] = "Georgian",
            ["kk"] = "Kazakh",
            ["km"] = "Khmer",
            ["kn"] = "Kannada",
            ["ko"] = "Korean",
            ["la"] = "Latin",
            ["lb"] = "Luxembourgish",
            ["ln"] = "Lingala",
            ["lo"] = "Lao",
            ["lt"] = "Lithuanian",
            ["lv"] = "Latvian",
            ["mg"] = "Malagasy",
            ["mi"] = "Maori",
            ["mk"] = "Macedonian",
            ["ml"] = "Malayalam",
            ["mn"] = "Mongolian",
            ["mr"] = "Marathi",
            ["ms"] = "Malay",
            ["mt"] = "Maltese",
            ["my"] = "Myanmar",
            ["ne"] = "Nepali",
            ["nl"] = "Dutch",
            ["nn"] = "Norwegian Nynorsk",
            ["no"] = "Norwegian",
            ["oc"] = "Occitan",
            ["pa"] = "Punjabi",
            ["pl"] = "Polish",
            ["ps"] = "Pashto",
            ["pt"] = "Portuguese",
            ["ro"] = "Romanian",
            ["ru"] = "Russian",
            ["sa"] = "Sanskrit",
            ["sd"] = "Sindhi",
            ["si"] = "Sinhala",
            ["sk"] = "Slovak",
            ["sl"] = "Slovenian",
            ["sn"] = "Shona",
            ["so"] = "Somali",
            ["sq"] = "Albanian",
            ["sr"] = "Serbian",
            ["su"] = "Sundanese",
            ["sv"] = "Swedish",
            ["sw"] = "Swahili",
            ["ta"] = "Tamil",
            ["te"] = "Telugu",
            ["tg"] = "Tajik",
            ["th"] = "Thai",
            ["tk"] = "Turkmen",
            ["tl"] = "Tagalog",
            ["tr"] = "Turkish",
            ["tt"] = "Tatar",
            ["uk"] = "Ukrainian",
            ["ur"] = "Urdu",
            ["uz"] = "Uzbek",
            ["vi"] = "Vietnamese",
            ["yi"] = "Yiddish",
            ["yo"] = "Yoruba",
            ["yue"] = "Cantonese",
            ["zh"] = "Chinese"
        };

    private static readonly TranscriptionLanguageChoice[] EnglishOnlyChoices =
    [
        new("en", "English")
    ];

    public static readonly IReadOnlyList<TranscriptionLanguageChoice> MultilingualChoices =
        WhisperLanguageCodes
            .Select(code => new TranscriptionLanguageChoice(code, LanguageNames[code]))
            .OrderBy(choice => choice.Code == "auto" ? 0 : 1)
            .ThenBy(choice => choice.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public static IReadOnlyList<TranscriptionLanguageChoice> ChoicesForModelPath(
        string modelPath,
        IEnumerable<LocalWhisperModel> localModels) =>
        IsEnglishOnlyModel(modelPath, localModels)
            ? EnglishOnlyChoices
            : MultilingualChoices;

    public static string CompatibleLanguageOrFallback(
        string modelPath,
        IEnumerable<LocalWhisperModel> localModels,
        string? selectedLanguage)
    {
        var choices = ChoicesForModelPath(modelPath, localModels);
        var normalizedLanguage = selectedLanguage?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(normalizedLanguage)
            && choices.Any(choice => string.Equals(choice.Code, normalizedLanguage, StringComparison.OrdinalIgnoreCase)))
        {
            return choices.First(choice =>
                string.Equals(choice.Code, normalizedLanguage, StringComparison.OrdinalIgnoreCase)).Code;
        }

        if (choices.Any(choice => choice.Code == "auto"))
        {
            return "auto";
        }

        return choices.FirstOrDefault(choice => choice.Code == "en")?.Code
            ?? choices.FirstOrDefault()?.Code
            ?? "en";
    }

    private static bool IsEnglishOnlyModel(
        string modelPath,
        IEnumerable<LocalWhisperModel> localModels)
    {
        var modelName = Path.GetFileNameWithoutExtension(modelPath.Trim());
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return false;
        }

        var catalogModel = WhisperModelCatalog.All.FirstOrDefault(model =>
            string.Equals(model.Name, modelName, StringComparison.OrdinalIgnoreCase));
        if (catalogModel is not null)
        {
            return !catalogModel.IsMultilingual;
        }

        var importedModel = localModels.FirstOrDefault(model =>
            string.Equals(model.Path, modelPath.Trim(), StringComparison.OrdinalIgnoreCase));
        return importedModel is null
            && modelName.EndsWith(".en", StringComparison.OrdinalIgnoreCase);
    }
}
