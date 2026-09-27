// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Windows;
using System.Windows.Media;
using Markdig;
using Markdig.Extensions.TaskLists;
using MarkDownEditor.Rendering.Styling;

// Alias WPF (UI)
using WpfBlock = System.Windows.Documents.Block;
using WpfTable = System.Windows.Documents.Table;
using WpfInline = System.Windows.Documents.Inline;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfParagraph = System.Windows.Documents.Paragraph;
using WpfSection = System.Windows.Documents.Section;
using WpfList = System.Windows.Documents.List;
using WpfListItem = System.Windows.Documents.ListItem;
using WpfRun = System.Windows.Documents.Run;
using WpfSpan = System.Windows.Documents.Span;
using WpfHyperlink = System.Windows.Documents.Hyperlink;
using WpfLineBreak = System.Windows.Documents.LineBreak;

// Alias Markdig (AST / Markdown)
using MdBlock = Markdig.Syntax.Block;
using MdTable = Markdig.Extensions.Tables.Table;
using MdInline = Markdig.Syntax.Inlines.Inline;

namespace MarkDownEditor.Rendering;

/// <summary>
/// Implémentation de référence de <see cref="IMarkdownRenderer"/>.
/// Parcourt l'arbre syntaxique produit par Markdig et construit un
/// <see cref="System.Windows.Documents.FlowDocument"/> WPF natif : aucun moteur HTML ou navigateur
/// embarqué n'est utilisé, conformément à la contrainte de fonctionnement
/// 100 % hors-ligne et légère du logiciel.
/// </summary>
public sealed class FlowDocumentMarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;
    private MarkdownRenderTheme _theme = MarkdownRenderTheme.Light;

    /// <summary>
    /// Crée le moteur de rendu. Le thème effectif est fourni à chaque
    /// appel de <see cref="Render"/> plutôt qu'à la construction, afin
    /// de permettre une bascule clair/sombre sans recréer le moteur.
    /// </summary>
    public FlowDocumentMarkdownRenderer()
    {
        _pipeline = MarkdownPipelineFactory.CreateDefault();
    }

    /// <inheritdoc />
    public System.Windows.Documents.FlowDocument Render(string markdownContent, MarkdownRenderTheme theme)
    {
        ArgumentNullException.ThrowIfNull(markdownContent);
        ArgumentNullException.ThrowIfNull(theme);

        _theme = theme;

        Markdig.Syntax.MarkdownDocument ast = Markdig.Markdown.Parse(markdownContent, _pipeline);

        var flowDocument = new System.Windows.Documents.FlowDocument
        {
            FontFamily = _theme.BodyFontFamily,
            FontSize = _theme.BaseFontSize,
            Foreground = _theme.TextBrush,
            Background = _theme.BackgroundBrush,
            PagePadding = new Thickness(0),
            TextAlignment = TextAlignment.Left,

            // Largeur de colonne de lecture confortable : le conteneur hôte
            // (FlowDocumentScrollViewer) centrera ce document dans la fenêtre.
            MaxPageWidth = 820,
        };

        foreach (MdBlock block in ast)
        {
            WpfBlock? rendered = RenderBlock(block);
            if (rendered is not null)
            {
                flowDocument.Blocks.Add(rendered);
            }
        }

        if (flowDocument.Blocks.Count == 0)
        {
            flowDocument.Blocks.Add(new WpfParagraph());
        }

        return flowDocument;
    }

    // ------------------------------------------------------------------
    // Rendu des blocs
    // ------------------------------------------------------------------

    private WpfBlock? RenderBlock(MdBlock block)
    {
        return block switch
        {
            Markdig.Syntax.HeadingBlock heading => RenderHeading(heading),
            Markdig.Syntax.ParagraphBlock paragraph => RenderParagraph(paragraph),
            Markdig.Syntax.QuoteBlock quote => RenderQuote(quote),
            Markdig.Syntax.FencedCodeBlock fenced => RenderCodeBlock(fenced),
            Markdig.Syntax.CodeBlock code => RenderCodeBlock(code),
            Markdig.Syntax.ListBlock list => RenderList(list),
            MdTable table => RenderTable(table),
            Markdig.Syntax.ThematicBreakBlock => RenderThematicBreak(),
            _ => RenderFallbackBlock(block),
        };
    }

    private WpfBlock RenderHeading(Markdig.Syntax.HeadingBlock heading)
    {
        double fontSize = heading.Level switch
        {
            1 => _theme.BaseFontSize * 2.00,
            2 => _theme.BaseFontSize * 1.65,
            3 => _theme.BaseFontSize * 1.35,
            4 => _theme.BaseFontSize * 1.15,
            5 => _theme.BaseFontSize * 1.05,
            _ => _theme.BaseFontSize * 0.95,
        };

        var paragraph = new WpfParagraph
        {
            FontSize = fontSize,
            FontWeight = FontWeights.SemiBold,
            Foreground = _theme.TextBrush,
            Margin = new Thickness(0, heading.Level == 1 ? 8 : 24, 0, 12),
            LineHeight = fontSize * 1.25,
        };

        // Sert d'ancre de navigation pour la future table des matières (Phase 3).
        paragraph.Name = BuildAnchorName(heading);

        if (heading.Level == 1)
        {
            paragraph.Padding = new Thickness(0, 0, 0, 12);
            paragraph.BorderBrush = _theme.BorderBrush;
            paragraph.BorderThickness = new Thickness(0, 0, 0, 1);
        }

        AppendInlines(paragraph, heading.Inline);
        return paragraph;
    }

    private WpfBlock RenderParagraph(Markdig.Syntax.ParagraphBlock paragraphBlock)
    {
        var paragraph = new WpfParagraph
        {
            Margin = new Thickness(0, 0, 0, 14),
            LineHeight = _theme.BaseFontSize * _theme.LineHeightMultiplier,
        };

        AppendInlines(paragraph, paragraphBlock.Inline);
        return paragraph;
    }

    private WpfBlock RenderQuote(Markdig.Syntax.QuoteBlock quote)
    {
        var innerSection = new WpfSection
        {
            Foreground = _theme.MutedTextBrush,
            FontStyle = FontStyles.Italic,
        };

        foreach (MdBlock child in quote)
        {
            WpfBlock? rendered = RenderBlock(child);
            if (rendered is not null)
            {
                innerSection.Blocks.Add(rendered);
            }
        }

        return new WpfSection
        {
            Margin = new Thickness(0, 4, 0, 16),
            Padding = new Thickness(16, 8, 16, 8),
            BorderBrush = _theme.BlockquoteBarBrush,
            BorderThickness = new Thickness(4, 0, 0, 0),
            Blocks = { innerSection },
        };
    }

    private WpfBlock RenderCodeBlock(Markdig.Syntax.CodeBlock codeBlock)
    {
        string codeText = ExtractCodeText(codeBlock);
        string? language = (codeBlock as Markdig.Syntax.FencedCodeBlock)?.Info;

        var codeParagraph = new WpfParagraph
        {
            FontFamily = _theme.MonospaceFontFamily,
            FontSize = _theme.BaseFontSize * 0.90,
            Foreground = _theme.CodeTextBrush,
            LineHeight = _theme.BaseFontSize * 1.45,
        };
        codeParagraph.Inlines.Add(new WpfRun(codeText));

        var container = new WpfSection
        {
            Background = _theme.CodeBackgroundBrush,
            BorderBrush = _theme.BorderBrush,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 4, 0, 16),
            Padding = new Thickness(16, 12, 16, 12),
        };

        if (!string.IsNullOrWhiteSpace(language))
        {
            var languageLabel = new WpfParagraph(new WpfRun(language.Trim().ToUpperInvariant()))
            {
                FontFamily = _theme.MonospaceFontFamily,
                FontSize = _theme.BaseFontSize * 0.70,
                Foreground = _theme.MutedTextBrush,
                Margin = new Thickness(0, 0, 0, 6),
                FontWeight = FontWeights.SemiBold,
            };
            container.Blocks.Add(languageLabel);
        }

        container.Blocks.Add(codeParagraph);
        return container;
    }

    private WpfBlock RenderList(Markdig.Syntax.ListBlock listBlock)
    {
        var list = new WpfList
        {
            MarkerStyle = listBlock.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc,
            Margin = new Thickness(0, 0, 0, 14),
            Padding = new Thickness(24, 0, 0, 0),
        };

        foreach (MdBlock itemBlock in listBlock)
        {
            if (itemBlock is not Markdig.Syntax.ListItemBlock listItem)
            {
                continue;
            }

            var wpfListItem = new WpfListItem
            {
                Margin = new Thickness(0, 2, 0, 2),
            };

            TaskList? taskListInfo = FindTaskList(listItem);

            foreach (MdBlock childBlock in listItem)
            {
                if (childBlock is Markdig.Syntax.ParagraphBlock childParagraph)
                {
                    var paragraph = new WpfParagraph
                    {
                        Margin = new Thickness(0),
                        LineHeight = _theme.BaseFontSize * _theme.LineHeightMultiplier,
                    };

                    if (taskListInfo is not null)
                    {
                        paragraph.Inlines.Add(new WpfRun(taskListInfo.Checked ? "☑ " : "☐ ")
                        {
                            Foreground = taskListInfo.Checked ? _theme.MutedTextBrush : _theme.TextBrush,
                        });
                    }

                    AppendInlines(paragraph, childParagraph.Inline);
                    wpfListItem.Blocks.Add(paragraph);
                }
                else
                {
                    WpfBlock? nested = RenderBlock(childBlock);
                    if (nested is not null)
                    {
                        wpfListItem.Blocks.Add(nested);
                    }
                }
            }

            list.ListItems.Add(wpfListItem);
        }

        return list;
    }

    private WpfBlock RenderTable(MdTable table)
    {
        var wpfTable = new WpfTable
        {
            Margin = new Thickness(0, 4, 0, 16),
            CellSpacing = 0,
            BorderBrush = _theme.BorderBrush,
            BorderThickness = new Thickness(1),
        };

        int columnCount = table.ColumnDefinitions.Count;
        for (int i = 0; i < columnCount; i++)
        {
            wpfTable.Columns.Add(new System.Windows.Documents.TableColumn());
        }

        var rowGroup = new System.Windows.Documents.TableRowGroup();
        wpfTable.RowGroups.Add(rowGroup);

        bool isFirstRow = true;
        int rowIndex = 0;

        foreach (Markdig.Extensions.Tables.TableRow tableRow in table.OfType<Markdig.Extensions.Tables.TableRow>())
        {
            var wpfRow = new WpfTableRow
            {
                Background = isFirstRow
                    ? _theme.TableHeaderBackgroundBrush
                    : (rowIndex % 2 == 1 ? _theme.TableAlternateRowBrush : Brushes.Transparent),
            };

            foreach (Markdig.Extensions.Tables.TableCell cell in tableRow.OfType<Markdig.Extensions.Tables.TableCell>())
            {
                var wpfCell = new WpfTableCell
                {
                    Padding = new Thickness(10, 6, 10, 6),
                    BorderBrush = _theme.BorderBrush,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                };

                foreach (MdBlock cellBlock in cell)
                {
                    if (cellBlock is Markdig.Syntax.ParagraphBlock cellParagraphBlock)
                    {
                        var cellParagraph = new WpfParagraph
                        {
                            Margin = new Thickness(0),
                            FontWeight = isFirstRow ? FontWeights.SemiBold : FontWeights.Normal,
                        };
                        AppendInlines(cellParagraph, cellParagraphBlock.Inline);
                        wpfCell.Blocks.Add(cellParagraph);
                    }
                }

                wpfRow.Cells.Add(wpfCell);
            }

            rowGroup.Rows.Add(wpfRow);
            isFirstRow = false;
            rowIndex++;
        }

        return wpfTable;
    }

    private WpfBlock RenderThematicBreak()
    {
        return new System.Windows.Documents.BlockUIContainer(new System.Windows.Controls.Border
        {
            BorderBrush = _theme.BorderBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Margin = new Thickness(0, 20, 0, 20),
        });
    }

    private WpfBlock RenderFallbackBlock(MdBlock block)
    {
        return new WpfParagraph(new WpfRun(string.Empty));
    }

    // ------------------------------------------------------------------
    // Rendu des éléments en ligne (gras, italique, liens, code, images…)
    // ------------------------------------------------------------------

    private void AppendInlines(WpfParagraph paragraph, Markdig.Syntax.Inlines.ContainerInline? container)
    {
        if (container is null)
        {
            return;
        }

        foreach (MdInline inline in container)
        {
            foreach (WpfInline? wpfInline in RenderInline(inline))
            {
                if (wpfInline is not null)
                {
                    paragraph.Inlines.Add(wpfInline);
                }
            }
        }
    }

    private IEnumerable<WpfInline?> RenderInline(MdInline inline)
    {
        switch (inline)
        {
            case Markdig.Syntax.Inlines.LiteralInline literal:
                yield return new WpfRun(literal.Content.ToString());
                break;

            case Markdig.Syntax.Inlines.EmphasisInline emphasis:
                yield return RenderEmphasis(emphasis);
                break;

            case Markdig.Syntax.Inlines.CodeInline code:
                yield return new WpfRun(code.Content)
                {
                    FontFamily = _theme.MonospaceFontFamily,
                    FontSize = _theme.BaseFontSize * 0.90,
                    Foreground = _theme.CodeTextBrush,
                    Background = _theme.CodeBackgroundBrush,
                };
                break;

            case Markdig.Syntax.Inlines.LineBreakInline:
                yield return new WpfLineBreak();
                break;

            case Markdig.Syntax.Inlines.LinkInline link when link.IsImage:
                yield return RenderImage(link);
                break;

            case Markdig.Syntax.Inlines.LinkInline link:
                yield return RenderLink(link);
                break;

            case Markdig.Syntax.Inlines.AutolinkInline autolink:
                yield return new WpfRun(autolink.Url);
                break;

            case TaskList:
                break;

            default:
                if (inline is Markdig.Syntax.Inlines.ContainerInline generic)
                {
                    foreach (MdInline child in generic)
                    {
                        foreach (WpfInline? rendered in RenderInline(child))
                        {
                            yield return rendered;
                        }
                    }
                }
                break;
        }
    }

    private WpfInline RenderEmphasis(Markdig.Syntax.Inlines.EmphasisInline emphasis)
    {
        var span = new WpfSpan();

        foreach (MdInline child in emphasis)
        {
            foreach (WpfInline? rendered in RenderInline(child))
            {
                if (rendered is not null)
                {
                    span.Inlines.Add(rendered);
                }
            }
        }

        if (emphasis.DelimiterChar == '~')
        {
            span.TextDecorations = TextDecorations.Strikethrough;
        }
        else if (emphasis.DelimiterCount >= 2)
        {
            span.FontWeight = FontWeights.Bold;
        }
        else
        {
            span.FontStyle = FontStyles.Italic;
        }

        return span;
    }

    private WpfInline RenderLink(Markdig.Syntax.Inlines.LinkInline link)
    {
        var hyperlink = new WpfHyperlink
        {
            Foreground = _theme.LinkBrush,
        };

        if (Uri.TryCreate(link.Url, UriKind.RelativeOrAbsolute, out Uri? uri))
        {
            hyperlink.NavigateUri = uri;
        }

        foreach (MdInline child in link)
        {
            foreach (WpfInline? rendered in RenderInline(child))
            {
                if (rendered is not null)
                {
                    hyperlink.Inlines.Add(rendered);
                }
            }
        }

        if (hyperlink.Inlines.Count == 0 && link.Url is not null)
        {
            hyperlink.Inlines.Add(new WpfRun(link.Url));
        }

        return hyperlink;
    }

    private WpfInline RenderImage(Markdig.Syntax.Inlines.LinkInline link)
    {
        string altText = ExtractPlainText(link);
        string label = string.IsNullOrWhiteSpace(altText)
            ? link.Url ?? string.Empty
            : altText;

        return new WpfRun($"🖼 {label}")
        {
            Foreground = _theme.MutedTextBrush,
            FontStyle = FontStyles.Italic,
        };
    }

    // ------------------------------------------------------------------
    // Utilitaires
    // ------------------------------------------------------------------

    private static string ExtractCodeText(Markdig.Syntax.CodeBlock codeBlock)
    {
        var lines = new List<string>();
        foreach (Markdig.Helpers.StringLine line in codeBlock.Lines.Lines)
        {
            lines.Add(line.ToString());
        }
        return string.Join(Environment.NewLine, lines);
    }

    private static string ExtractPlainText(Markdig.Syntax.Inlines.ContainerInline container)
    {
        var builder = new System.Text.StringBuilder();
        foreach (MdInline inline in container)
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

    private static TaskList? FindTaskList(Markdig.Syntax.ListItemBlock listItem)
    {
        foreach (MdBlock block in listItem)
        {
            if (block is Markdig.Syntax.ParagraphBlock { Inline: not null } paragraph)
            {
                foreach (MdInline inline in paragraph.Inline)
                {
                    if (inline is TaskList taskList)
                    {
                        return taskList;
                    }
                }
            }
        }
        return null;
    }

    private static string BuildAnchorName(Markdig.Syntax.HeadingBlock heading)
    {
        string text = heading.Inline is null
            ? string.Empty
            : ExtractPlainText(heading.Inline);

        return HeadingAnchorNaming.Build(text, heading.Line);
    }
}