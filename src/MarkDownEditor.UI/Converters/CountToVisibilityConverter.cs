// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarkDownEditor.UI.Converters;

/// <summary>
/// Convertit un nombre d'éléments en <see cref="Visibility"/> : visible
/// lorsque la collection est vide (zéro élément), sinon réduit. Utilisé
/// pour afficher le message « Aucun fichier récent » uniquement quand la
/// liste des fichiers récents est vide.
/// </summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int count = value is int intValue ? intValue : 0;
        return count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("CountToVisibilityConverter ne prend en charge que la conversion à sens unique.");
    }
}
