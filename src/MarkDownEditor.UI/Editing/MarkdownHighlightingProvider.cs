// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MarkDownEditor.Rendering.Styling;

namespace MarkDownEditor.UI.Editing;

/// <summary>
/// Implémentation de <see cref="IMarkdownHighlightingProvider"/> : charge
/// la définition XSHD embarquée (<c>Editing/Markdown.xshd</c>) puis
/// recolore chacune de ses règles à partir de la palette du
/// <see cref="MarkdownRenderTheme"/> actif, afin que le mode Édition
/// reste visuellement cohérent avec le mode Lecture et la bascule
/// clair/sombre.
/// </summary>
public sealed class MarkdownHighlightingProvider : IMarkdownHighlightingProvider
{
    private const string XshdResourceName = "MarkDownEditor.UI.Editing.Markdown.xshd";

    private IHighlightingDefinition? _lightDefinition;
    private IHighlightingDefinition? _darkDefinition;

    /// <inheritdoc />
    public IHighlightingDefinition GetDefinition(MarkdownRenderTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        bool isDark = ReferenceEquals(theme, MarkdownRenderTheme.Dark);

        if (isDark)
        {
            _darkDefinition ??= BuildDefinition(theme);
            return _darkDefinition;
        }

        _lightDefinition ??= BuildDefinition(theme);
        return _lightDefinition;
    }

    private static IHighlightingDefinition BuildDefinition(MarkdownRenderTheme theme)
    {
        IHighlightingDefinition definition = LoadFromEmbeddedResource();
        ApplyThemeColors(definition, theme);
        return definition;
    }

    private static IHighlightingDefinition LoadFromEmbeddedResource()
    {
        Assembly assembly = typeof(MarkdownHighlightingProvider).Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(XshdResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Ressource embarquée introuvable : {XshdResourceName}. " +
                "Vérifier que Markdown.xshd est bien inclus en tant que EmbeddedResource.");
        }

        using XmlReader reader = XmlReader.Create(stream);
        XshdSyntaxDefinition xshd = HighlightingLoader.LoadXshd(reader);
        return HighlightingLoader.Load(xshd, HighlightingManager.Instance);
    }

    private static void ApplyThemeColors(IHighlightingDefinition definition, MarkdownRenderTheme theme)
    {
        SetColorForeground(definition, "Heading", theme.LinkBrush);
        SetColorForeground(definition, "Emphasis", theme.TextBrush);
        SetColorForeground(definition, "StrongEmphasis", theme.TextBrush);
        SetColorForeground(definition, "Strikethrough", theme.MutedTextBrush);
        SetColorForeground(definition, "InlineCode", theme.CodeTextBrush);
        SetColorForeground(definition, "CodeFence", theme.MutedTextBrush);
        SetColorForeground(definition, "Blockquote", theme.MutedTextBrush);
        SetColorForeground(definition, "ListMarker", theme.LinkBrush);
        SetColorForeground(definition, "Link", theme.LinkBrush);
        SetColorForeground(definition, "HorizontalRule", theme.MutedTextBrush);
        SetColorForeground(definition, "TableDelimiter", theme.MutedTextBrush);
    }

    private static void SetColorForeground(IHighlightingDefinition definition, string colorName, Brush brush)
    {
        HighlightingColor? color = definition.GetNamedColor(colorName);
        if (color is null)
        {
            return;
        }

        if (brush is SolidColorBrush solidColorBrush)
        {
            color.Foreground = new SimpleHighlightingBrush(solidColorBrush.Color);
        }
    }
}