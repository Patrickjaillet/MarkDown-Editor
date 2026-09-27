// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarkDownEditor.UI.Converters;

/// <summary>
/// Convertit le niveau d'un titre (1 à 6, voir
/// <c>TableOfContentsEntry.Level</c>) en une marge gauche croissante,
/// afin de représenter visuellement la hiérarchie des titres dans la
/// table des matières de la barre latérale (voir ROADMAP.md, §6 Phase 3).
/// Un titre de niveau 1 n'a aucune indentation ; chaque niveau
/// supplémentaire ajoute 12 pixels.
/// </summary>
public sealed class HeadingLevelToIndentConverter : IValueConverter
{
    private const double IndentStepInPixels = 12d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int level = value is int intValue ? intValue : 1;
        double leftMargin = Math.Max(0, level - 1) * IndentStepInPixels;
        return new Thickness(leftMargin, 2, 4, 2);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("HeadingLevelToIndentConverter ne prend en charge que la conversion à sens unique.");
    }
}
