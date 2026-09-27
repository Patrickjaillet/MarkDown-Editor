// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core;

/// <summary>
/// Entrée de la table des matières générée automatiquement à partir des
/// titres (<c>#</c> à <c>######</c>) d'un document Markdown.
/// </summary>
/// <param name="Level">Niveau du titre, de 1 (<c>#</c>) à 6 (<c>######</c>).</param>
/// <param name="Text">Texte brut du titre, sans mise en forme Markdown.</param>
/// <param name="AnchorName">
/// Nom d'ancre unique correspondant au paragraphe du titre dans le
/// <c>FlowDocument</c> rendu (voir
/// <c>FlowDocumentMarkdownRenderer.BuildAnchorName</c>), utilisé pour la
/// navigation par clic depuis la table des matières.
/// </param>
public sealed record TableOfContentsEntry(int Level, string Text, string AnchorName);
