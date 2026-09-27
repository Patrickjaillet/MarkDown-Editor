// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using Markdig;
using Markdig.Syntax;
using MarkDownEditor.Core;

namespace MarkDownEditor.Rendering;

/// <summary>
/// Implémentation de <see cref="IMarkdownOutlineService"/> : parcourt
/// l'arbre syntaxique Markdig (même pipeline partagé que le reste du
/// logiciel, voir <see cref="MarkdownPipelineFactory"/>) et collecte
/// chaque bloc de titre.
/// </summary>
public sealed class MarkdownOutlineService : IMarkdownOutlineService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownOutlineService()
    {
        _pipeline = MarkdownPipelineFactory.CreateDefault();
    }

    /// <inheritdoc />
    public IReadOnlyList<TableOfContentsEntry> ExtractHeadings(string markdownContent)
    {
        ArgumentNullException.ThrowIfNull(markdownContent);

        Markdig.Syntax.MarkdownDocument ast = Markdig.Markdown.Parse(markdownContent, _pipeline);

        var entries = new List<TableOfContentsEntry>();

        foreach (Block block in ast)
        {
            if (block is not HeadingBlock heading)
            {
                continue;
            }

            string text = heading.Inline is null
                ? string.Empty
                : ExtractPlainText(heading.Inline);

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            string anchorName = HeadingAnchorNaming.Build(text, heading.Line);
            entries.Add(new TableOfContentsEntry(heading.Level, text, anchorName));
        }

        return entries;
    }

    private static string ExtractPlainText(Markdig.Syntax.Inlines.ContainerInline container)
    {
        var builder = new System.Text.StringBuilder();
        foreach (Markdig.Syntax.Inlines.Inline inline in container)
        {
            if (inline is Markdig.Syntax.Inlines.LiteralInline literal)
            {
                builder.Append(literal.Content.ToString());
            }
            else if (inline is Markdig.Syntax.Inlines.ContainerInline nested)
            {
                builder.Append(ExtractPlainText(nested));
            }
        }
        return builder.ToString();
    }
}
