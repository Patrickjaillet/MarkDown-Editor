// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core;

/// <summary>
/// Thème visuel de l'application.
/// </summary>
public enum AppTheme
{
    /// <summary>Suit le thème clair/sombre configuré dans Windows.</summary>
    Auto,

    /// <summary>Thème clair forcé, indépendamment du thème Windows.</summary>
    Light,

    /// <summary>Thème sombre forcé, indépendamment du thème Windows.</summary>
    Dark,
}
