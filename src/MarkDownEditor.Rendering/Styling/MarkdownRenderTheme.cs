// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Windows;
using System.Windows.Media;

namespace MarkDownEditor.Rendering.Styling;

/// <summary>
/// Décrit l'ensemble des grandeurs typographiques et colorimétriques
/// utilisées par le moteur de rendu Markdown, pour un thème donné
/// (clair ou sombre). Regrouper ces valeurs ici évite toute couleur
/// ou taille codée en dur dans le générateur de <c>FlowDocument</c>.
/// </summary>
public sealed class MarkdownRenderTheme
{
    /// <summary>Police principale du corps de texte.</summary>
    public required FontFamily BodyFontFamily { get; init; }

    /// <summary>Police à chasse fixe utilisée pour le code.</summary>
    public required FontFamily MonospaceFontFamily { get; init; }

    /// <summary>Taille de police de base du corps de texte (points WPF).</summary>
    public required double BaseFontSize { get; init; }

    /// <summary>Interlignage du corps de texte (multiplicateur appliqué à la taille de police).</summary>
    public required double LineHeightMultiplier { get; init; }

    /// <summary>Couleur du texte principal.</summary>
    public required Brush TextBrush { get; init; }

    /// <summary>Couleur du texte atténué (légendes, métadonnées).</summary>
    public required Brush MutedTextBrush { get; init; }

    /// <summary>Couleur de fond du document.</summary>
    public required Brush BackgroundBrush { get; init; }

    /// <summary>Couleur de fond des blocs de code et du code en ligne.</summary>
    public required Brush CodeBackgroundBrush { get; init; }

    /// <summary>Couleur du texte à l'intérieur des blocs de code.</summary>
    public required Brush CodeTextBrush { get; init; }

    /// <summary>Couleur de la bordure des blocs de code et des tableaux.</summary>
    public required Brush BorderBrush { get; init; }

    /// <summary>Couleur du filet vertical des citations (blockquote).</summary>
    public required Brush BlockquoteBarBrush { get; init; }

    /// <summary>Couleur des liens hypertextes.</summary>
    public required Brush LinkBrush { get; init; }

    /// <summary>Couleur de fond alternée des lignes de tableau (zébrage léger).</summary>
    public required Brush TableAlternateRowBrush { get; init; }

    /// <summary>Couleur de fond de l'en-tête de tableau.</summary>
    public required Brush TableHeaderBackgroundBrush { get; init; }

    /// <summary>
    /// Thème clair par défaut : fond quasi blanc, texte anthracite,
    /// blocs de code légèrement grisés — pensé pour un confort de
    /// lecture prolongée plutôt qu'un contraste maximal.
    /// </summary>
    public static MarkdownRenderTheme Light { get; } = new()
    {
        BodyFontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Calibri"),
        MonospaceFontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
        BaseFontSize = 16,
        LineHeightMultiplier = 1.65,
        TextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x1F, 0x22, 0x28))),
        MutedTextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80))),
        BackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF))),
        CodeBackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6))),
        CodeTextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x1F, 0x22, 0x28))),
        BorderBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE2, 0xE4, 0xE9))),
        BlockquoteBarBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xC7, 0xCC, 0xD6))),
        LinkBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x2F, 0x6F, 0xED))),
        TableAlternateRowBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFB))),
        TableHeaderBackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xF0, 0xF1, 0xF4))),
    };

    /// <summary>
    /// Thème sombre par défaut : fond charbon, texte quasi blanc cassé,
    /// blocs de code légèrement plus clairs que le fond pour rester lisibles.
    /// </summary>
    public static MarkdownRenderTheme Dark { get; } = new()
    {
        BodyFontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Calibri"),
        MonospaceFontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
        BaseFontSize = 16,
        LineHeightMultiplier = 1.65,
        TextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE7, 0xE9, 0xEC))),
        MutedTextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x9A, 0xA1, 0xAC))),
        BackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x1A, 0x1C, 0x20))),
        CodeBackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x26, 0x29, 0x2E))),
        CodeTextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE7, 0xE9, 0xEC))),
        BorderBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x3A, 0x3D, 0x44))),
        BlockquoteBarBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x4A, 0x4E, 0x57))),
        LinkBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x6F, 0xA8, 0xF5))),
        TableAlternateRowBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x20, 0x22, 0x27))),
        TableHeaderBackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x24, 0x26, 0x2C))),
    };

    private static SolidColorBrush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}
