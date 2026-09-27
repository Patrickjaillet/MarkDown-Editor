// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core;

/// <summary>
/// Préférences utilisateur persistées localement, dans le dossier de
/// l'application (<c>config/settings.local.json</c>), jamais dans
/// <c>%APPDATA%</c>, conformément à la contrainte de portabilité stricte.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Langue d'interface souhaitée : <c>"auto"</c> (détection système),
    /// <c>"fr"</c> ou <c>"en"</c>.
    /// </summary>
    public string Language { get; set; } = "auto";

    /// <summary>
    /// Thème visuel souhaité : <c>"auto"</c>, <c>"light"</c> ou <c>"dark"</c>.
    /// </summary>
    public string Theme { get; set; } = "auto";

    /// <summary>
    /// Mode d'affichage par défaut au démarrage : <c>"Reading"</c>,
    /// <c>"Editing"</c> ou <c>"Split"</c> (voir <see cref="DocumentViewMode"/>).
    /// </summary>
    public string ViewMode { get; set; } = "Reading";

    /// <summary>Police utilisée pour le mode Lecture.</summary>
    public string ReadingFont { get; set; } = "Segoe UI Variable";

    /// <summary>Taille de police (points WPF) utilisée pour le mode Lecture.</summary>
    public double ReadingFontSize { get; set; } = 16;

    /// <summary>
    /// Chemins absolus des fichiers récemment ouverts, du plus récent au
    /// plus ancien. Persistés localement, jamais synchronisés en ligne.
    /// </summary>
    public List<string> RecentFiles { get; set; } = [];

    /// <summary>
    /// Nombre maximal d'entrées conservées dans <see cref="RecentFiles"/>.
    /// </summary>
    public const int MaxRecentFiles = 10;

    /// <summary>
    /// Ajoute (ou fait remonter) un fichier en tête de la liste des
    /// fichiers récents, en respectant la limite <see cref="MaxRecentFiles"/>
    /// et en évitant les doublons (comparaison insensible à la casse,
    /// adaptée aux chemins Windows).
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier à enregistrer.</param>
    public void PushRecentFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        RecentFiles.RemoveAll(existing => string.Equals(existing, filePath, StringComparison.OrdinalIgnoreCase));
        RecentFiles.Insert(0, filePath);

        if (RecentFiles.Count > MaxRecentFiles)
        {
            RecentFiles.RemoveRange(MaxRecentFiles, RecentFiles.Count - MaxRecentFiles);
        }
    }

    /// <summary>
    /// Retire un fichier de la liste des fichiers récents, typiquement
    /// après échec d'ouverture (fichier déplacé ou supprimé).
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier à retirer.</param>
    public void RemoveRecentFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        RecentFiles.RemoveAll(existing => string.Equals(existing, filePath, StringComparison.OrdinalIgnoreCase));
    }
}
