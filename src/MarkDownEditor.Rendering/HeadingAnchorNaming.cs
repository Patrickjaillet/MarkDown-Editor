// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Rendering;

/// <summary>
/// Génère un nom d'ancre stable et unique pour un titre Markdown, à
/// partir de son texte brut. Utilisé à la fois par
/// <see cref="FlowDocumentMarkdownRenderer"/> (nom du paragraphe WPF
/// correspondant, cible de navigation) et par
/// <see cref="MarkdownOutlineService"/> (table des matières), afin que
/// les deux restent toujours en accord sur le même identifiant pour un
/// même titre.
/// </summary>
internal static class HeadingAnchorNaming
{
    /// <summary>
    /// Construit un nom d'ancre à partir du texte brut d'un titre et de
    /// son numéro de ligne dans le document source (utilisé uniquement en
    /// repli si le texte ne contient aucun caractère alphanumérique).
    /// </summary>
    /// <param name="headingText">Texte brut du titre (sans mise en forme).</param>
    /// <param name="sourceLine">
    /// Numéro de ligne du titre dans le document source, utilisé pour
    /// garantir un nom d'ancre non vide même pour un titre sans texte
    /// alphanumérique (ex. composé uniquement d'émojis ou de ponctuation).
    /// </param>
    public static string Build(string headingText, int sourceLine)
    {
        ArgumentNullException.ThrowIfNull(headingText);

        // Important : la propriété WPF FrameworkContentElement.Name (utilisée
        // par le moteur de rendu pour poser une ancre navigable) doit être un
        // identifiant valide : uniquement lettres ASCII, chiffres et
        // underscore, sans commencer par un chiffre. Toute autre valeur
        // (tiret, espace, caractère accentué, etc.) fait lever une
        // ArgumentException à l'affectation de `Name`, ce qui empêchait le
        // mode Lecture de fonctionner dès qu'un titre était présent dans le
        // document. Le séparateur est donc '_' et non '-', et les
        // caractères non-ASCII sont supprimés plutôt que conservés tels
        // quels.
        string slug = new string(headingText
            .ToLowerInvariant()
            .Select(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' ? c : '_')
            .ToArray());

        while (slug.Contains("__"))
        {
            slug = slug.Replace("__", "_");
        }

        slug = slug.Trim('_');

        if (string.IsNullOrEmpty(slug) || !char.IsAsciiLetter(slug[0]))
        {
            return $"heading_{sourceLine}";
        }

        return $"heading_{slug}";
    }
}
