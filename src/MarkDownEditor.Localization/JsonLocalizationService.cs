// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Text.Json;

namespace MarkDownEditor.Localization;

/// <summary>
/// Implémentation de <see cref="ILocalizationService"/> qui charge les
/// traductions depuis des fichiers JSON présents dans le dossier de
/// l'application (<c>assets/i18n/en.json</c>, <c>assets/i18n/fr.json</c>).
/// </summary>
public sealed class JsonLocalizationService : ILocalizationService
{
    private readonly string _i18nDirectory;
    private Dictionary<string, string> _translations;

    /// <inheritdoc />
    public AppLanguage CurrentLanguage { get; private set; }

    /// <inheritdoc />
    public event EventHandler<AppLanguage>? LanguageChanged;

    /// <summary>
    /// Crée le service de localisation.
    /// </summary>
    /// <param name="i18nDirectory">
    /// Dossier absolu contenant les fichiers <c>en.json</c> et <c>fr.json</c>.
    /// </param>
    public JsonLocalizationService(string i18nDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(i18nDirectory);

        _i18nDirectory = i18nDirectory;
        _translations = new Dictionary<string, string>(StringComparer.Ordinal);
        CurrentLanguage = AppLanguage.English;
    }

    /// <inheritdoc />
    public async Task SetLanguageAsync(AppLanguage language)
    {
        string fileName = language switch
        {
            AppLanguage.French => "fr.json",
            AppLanguage.English => "en.json",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Langue non prise en charge."),
        };

        string filePath = Path.Combine(_i18nDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Fichier de traduction introuvable : {filePath}",
                filePath);
        }

        await using FileStream stream = File.OpenRead(filePath);
        using JsonDocument document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        var flattened = new Dictionary<string, string>(StringComparer.Ordinal);
        Flatten(document.RootElement, prefix: string.Empty, flattened);

        _translations = flattened;
        CurrentLanguage = language;

        LanguageChanged?.Invoke(this, language);
    }

    /// <inheritdoc />
    public string Translate(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return _translations.TryGetValue(key, out string? value)
            ? value
            : $"[{key}]";
    }

    /// <inheritdoc />
    public string Translate(string key, params object[] args)
    {
        string template = Translate(key);
        return args.Length == 0 ? template : string.Format(template, args);
    }

    private static void Flatten(JsonElement element, string prefix, Dictionary<string, string> destination)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            // La section "_meta" décrit le fichier lui-même et n'est pas une clé traduisible.
            if (prefix.Length == 0 && string.Equals(property.Name, "_meta", StringComparison.Ordinal))
            {
                continue;
            }

            string fullKey = prefix.Length == 0 ? property.Name : $"{prefix}.{property.Name}";

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    Flatten(property.Value, fullKey, destination);
                    break;
                case JsonValueKind.String:
                    destination[fullKey] = property.Value.GetString() ?? string.Empty;
                    break;
                default:
                    // Toute autre forme (nombre, booléen...) n'est pas attendue
                    // dans les fichiers i18n mais est ignorée sans échec bloquant.
                    break;
            }
        }
    }
}
