// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Windows.Documents;
using MarkDownEditor.Rendering.Styling;

namespace MarkDownEditor.Rendering;

/// <summary>
/// Contrat du moteur de rendu chargé de transformer un contenu Markdown
/// en <see cref="FlowDocument"/> WPF, avec une typographie soignée et une
/// mise en page aérée proche des interfaces de lecture modernes.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Transforme le contenu Markdown donné en document de flux WPF prêt
    /// à être affiché dans un <c>FlowDocumentScrollViewer</c> (ou équivalent).
    /// </summary>
    /// <param name="markdownContent">Contenu Markdown brut.</param>
    /// <param name="theme">
    /// Palette de styles à appliquer ; permet au même moteur de rendu de
    /// s'adapter dynamiquement au thème clair/sombre actif sans être
    /// reconstruit à chaque bascule.
    /// </param>
    /// <returns>Un <see cref="FlowDocument"/> représentant le rendu du document.</returns>
    FlowDocument Render(string markdownContent, MarkdownRenderTheme theme);
}
