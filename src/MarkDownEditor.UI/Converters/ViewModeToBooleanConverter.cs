// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Globalization;
using System.Windows.Data;
using MarkDownEditor.Core;

namespace MarkDownEditor.UI.Converters;

/// <summary>
/// Convertit le <see cref="DocumentViewMode"/> actif en booléen indiquant
/// si le bouton représentant le mode donné en paramètre doit apparaître
/// enfoncé. Utilisé par les <c>ui:ToggleButton</c> de bascule Lecture /
/// Édition / Vue partagée, dont l'état est piloté en lecture seule par le
/// ViewModel (la commande gère le changement, pas la case à cocher
/// elle-même).
/// </summary>
public sealed class ViewModeToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentViewMode currentMode || parameter is not string modeName)
        {
            return false;
        }

        return Enum.TryParse(modeName, out DocumentViewMode targetMode) && currentMode == targetMode;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException(
            "ViewModeToBooleanConverter est utilisé en liaison à sens unique (Mode=OneWay) ; " +
            "le changement de mode passe exclusivement par SetViewModeCommand.");
    }
}
