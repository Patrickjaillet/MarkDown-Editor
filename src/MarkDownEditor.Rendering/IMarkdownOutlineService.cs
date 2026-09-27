// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using MarkDownEditor.Core;

namespace MarkDownEditor.Rendering;

/// <summary>
/// Extrait la structure des titres (table des matières) d'un document
/// Markdown, avec des noms d'ancre identiques à ceux posés par
/// <see cref="FlowDocumentMarkdownRenderer"/> sur le <c>FlowDocument</c>
/// rendu, afin de permettre la navigation par clic depuis la barre
/// latérale.
/// </summary>
public interface IMarkdownOutlineService
{
    /// <summary>
    /// Analyse le contenu Markdown donné et retourne la liste ordonnée de
    /// ses titres (<c>#</c> à <c>######</c>), dans l'ordre du document.
    /// </summary>
    /// <param name="markdownContent">Contenu Markdown brut.</param>
    IReadOnlyList<TableOfContentsEntry> ExtractHeadings(string markdownContent);
}
