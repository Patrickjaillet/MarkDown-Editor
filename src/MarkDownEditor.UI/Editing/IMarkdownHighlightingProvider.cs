// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using ICSharpCode.AvalonEdit.Highlighting;
using MarkDownEditor.Rendering.Styling;

namespace MarkDownEditor.UI.Editing;

/// <summary>
/// Fournit la définition de coloration syntaxique Markdown utilisée par
/// l'éditeur AvalonEdit du mode Édition, avec une palette cohérente avec
/// le thème de rendu actif (<see cref="MarkdownRenderTheme"/>).
/// </summary>
public interface IMarkdownHighlightingProvider
{
    /// <summary>
    /// Retourne la définition de coloration syntaxique Markdown, avec ses
    /// couleurs recalculées à partir du thème donné.
    /// </summary>
    /// <param name="theme">Thème de rendu actif (clair ou sombre).</param>
    IHighlightingDefinition GetDefinition(MarkdownRenderTheme theme);
}
