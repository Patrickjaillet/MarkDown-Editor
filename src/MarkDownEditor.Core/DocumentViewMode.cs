// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core;

/// <summary>
/// Mode d'affichage du document actif dans la fenêtre principale.
/// </summary>
public enum DocumentViewMode
{
    /// <summary>Rendu Markdown seul (style Claude.ai), lecture seule.</summary>
    Reading,

    /// <summary>Texte brut seul, avec coloration syntaxique et édition.</summary>
    Editing,

    /// <summary>Édition et rendu affichés côte à côte, synchronisés.</summary>
    Split,
}
