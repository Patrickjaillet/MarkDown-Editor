// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using MarkDownEditor.Core;
using MarkDownEditor.Rendering.Styling;

namespace MarkDownEditor.UI.Services;

/// <summary>
/// Pilote le thème visuel de l'application : thème système WPF-UI
/// (fenêtre, contrôles) et palette de rendu Markdown associée
/// (<see cref="MarkdownRenderTheme"/>), qui doivent toujours rester cohérents.
/// </summary>
public interface IThemeService
{
    /// <summary>Thème actuellement demandé par l'utilisateur (peut être <see cref="AppTheme.Auto"/>).</summary>
    AppTheme RequestedTheme { get; }

    /// <summary>
    /// Thème effectivement appliqué (clair ou sombre), résolu à partir de
    /// <see cref="RequestedTheme"/> et, si nécessaire, du thème Windows actuel.
    /// </summary>
    bool IsDarkModeActive { get; }

    /// <summary>Palette de rendu Markdown correspondant au thème effectif actuel.</summary>
    MarkdownRenderTheme CurrentRenderTheme { get; }

    /// <summary>Se déclenche chaque fois que le thème effectif change.</summary>
    event EventHandler? EffectiveThemeChanged;

    /// <summary>
    /// Applique le thème demandé à l'ensemble de l'application (fenêtres
    /// WPF-UI et rendu Markdown).
    /// </summary>
    /// <param name="theme">Thème souhaité.</param>
    void ApplyTheme(AppTheme theme);
}
