// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Rendering;

/// <summary>
/// Exporte un document Markdown vers un fichier HTML statique et
/// autonome : toute la mise en forme (CSS) est intégrée dans le
/// fichier généré, sans feuille de style externe, sans police
/// distante (Google Fonts ou autre) et sans script, conformément à la
/// contrainte de fonctionnement 100 % hors-ligne du logiciel. Le
/// fichier produit reste lisible et fidèle même ouvert plus tard sans
/// aucune connexion réseau ni dépendance à MarkDown Editor lui-même.
/// </summary>
public interface IHtmlExportService
{
    /// <summary>
    /// Convertit le contenu Markdown donné en une page HTML complète et
    /// autonome (balises <c>&lt;html&gt;</c>, <c>&lt;head&gt;</c> avec CSS
    /// intégré, <c>&lt;body&gt;</c>), puis l'écrit à l'emplacement donné.
    /// </summary>
    /// <param name="markdownContent">Contenu Markdown brut à exporter.</param>
    /// <param name="documentTitle">
    /// Titre du document, utilisé pour la balise <c>&lt;title&gt;</c> de
    /// la page générée.
    /// </param>
    /// <param name="targetFilePath">Chemin absolu du fichier <c>.html</c> à écrire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    Task ExportAsync(
        string markdownContent,
        string documentTitle,
        string targetFilePath,
        CancellationToken cancellationToken = default);
}
