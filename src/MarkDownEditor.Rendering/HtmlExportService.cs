// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.IO;
using System.Net;
using System.Text;
using Markdig;

namespace MarkDownEditor.Rendering;

/// <summary>
/// Implémentation de <see cref="IHtmlExportService"/> : convertit le
/// Markdown en HTML via le pipeline Markdig partagé
/// (<see cref="MarkdownPipelineFactory"/>) et enveloppe le résultat dans
/// un document HTML5 complet, avec une feuille de style intégrée
/// inspirée de la palette de rendu Lecture (<c>MarkdownRenderTheme</c>),
/// claire par défaut et basculant en sombre via
/// <c>prefers-color-scheme</c> selon les préférences du système du
/// lecteur — aucune dépendance réseau, aucune police distante.
/// </summary>
public sealed class HtmlExportService : IHtmlExportService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private readonly MarkdownPipeline _pipeline;

    public HtmlExportService()
    {
        _pipeline = MarkdownPipelineFactory.CreateDefault();
    }

    /// <inheritdoc />
    public async Task ExportAsync(
        string markdownContent,
        string documentTitle,
        string targetFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(markdownContent);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFilePath);

        string safeTitle = string.IsNullOrWhiteSpace(documentTitle)
            ? "MarkDown Editor"
            : documentTitle;

        string bodyHtml = Markdig.Markdown.ToHtml(markdownContent, _pipeline);
        string document = BuildHtmlDocument(safeTitle, bodyHtml);

        string? directory = Path.GetDirectoryName(targetFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(targetFilePath, document, Utf8NoBom, cancellationToken).ConfigureAwait(false);
    }

    private static string BuildHtmlDocument(string title, string bodyHtml)
    {
        string encodedTitle = WebUtility.HtmlEncode(title);

        return $$"""
            <!DOCTYPE html>
            <html lang="fr">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{encodedTitle}}</title>
            <style>
            {{Css}}
            </style>
            </head>
            <body>
            <main class="markdown-body">
            {{bodyHtml}}
            </main>
            </body>
            </html>
            """;
    }

    /// <summary>
    /// Feuille de style intégrée, sans dépendance externe : polices
    /// système uniquement (mêmes familles que le mode Lecture natif),
    /// couleurs alignées sur <c>MarkdownRenderTheme.Light</c>/<c>Dark</c>,
    /// bascule automatique via <c>prefers-color-scheme</c>.
    /// </summary>
    private const string Css = """
        :root {
            color-scheme: light dark;
            --md-text: #1F2228;
            --md-muted: #6B7280;
            --md-background: #FFFFFF;
            --md-code-bg: #F3F4F6;
            --md-code-text: #1F2228;
            --md-border: #E2E4E9;
            --md-blockquote-bar: #C7CCD6;
            --md-link: #2F6FED;
            --md-table-alt-row: #FAFAFB;
            --md-table-header-bg: #F0F1F4;
        }

        @media (prefers-color-scheme: dark) {
            :root {
                --md-text: #E7E9EC;
                --md-muted: #9AA1AC;
                --md-background: #1A1C20;
                --md-code-bg: #26292E;
                --md-code-text: #E7E9EC;
                --md-border: #3A3D44;
                --md-blockquote-bar: #4A4E57;
                --md-link: #6FA8F5;
                --md-table-alt-row: #202227;
                --md-table-header-bg: #24262C;
            }
        }

        * {
            box-sizing: border-box;
        }

        html, body {
            margin: 0;
            padding: 0;
            background: var(--md-background);
        }

        body {
            font-family: "Segoe UI Variable", "Segoe UI", Calibri, sans-serif;
            font-size: 16px;
            line-height: 1.65;
            color: var(--md-text);
        }

        main.markdown-body {
            max-width: 820px;
            margin: 0 auto;
            padding: 48px 24px 96px;
        }

        h1, h2, h3, h4, h5, h6 {
            font-weight: 600;
            line-height: 1.25;
            color: var(--md-text);
        }

        h1 {
            font-size: 2em;
            margin: 8px 0 12px;
            padding-bottom: 12px;
            border-bottom: 1px solid var(--md-border);
        }

        h2 { font-size: 1.65em; margin: 24px 0 12px; }
        h3 { font-size: 1.35em; margin: 24px 0 12px; }
        h4 { font-size: 1.15em; margin: 24px 0 12px; }
        h5 { font-size: 1.05em; margin: 24px 0 12px; }
        h6 { font-size: 0.95em; margin: 24px 0 12px; }

        p {
            margin: 0 0 14px;
        }

        a {
            color: var(--md-link);
        }

        blockquote {
            margin: 4px 0 16px;
            padding: 8px 16px;
            border-left: 4px solid var(--md-blockquote-bar);
            color: var(--md-muted);
            font-style: italic;
        }

        blockquote p:last-child {
            margin-bottom: 0;
        }

        code {
            font-family: "Cascadia Code", Consolas, "Courier New", monospace;
            font-size: 0.9em;
            color: var(--md-code-text);
            background: var(--md-code-bg);
            padding: 0.15em 0.35em;
            border-radius: 4px;
        }

        pre {
            margin: 4px 0 16px;
            padding: 16px;
            background: var(--md-code-bg);
            border: 1px solid var(--md-border);
            overflow-x: auto;
        }

        pre code {
            background: none;
            padding: 0;
            border-radius: 0;
            font-size: 0.9em;
            line-height: 1.45;
        }

        ul, ol {
            margin: 0 0 14px;
            padding-left: 24px;
        }

        li {
            margin: 2px 0;
        }

        li > p {
            margin: 0;
        }

        hr {
            margin: 20px 0;
            border: none;
            border-top: 1px solid var(--md-border);
        }

        table {
            border-collapse: collapse;
            margin: 4px 0 16px;
            width: 100%;
            border: 1px solid var(--md-border);
        }

        th, td {
            padding: 6px 10px;
            border-bottom: 1px solid var(--md-border);
            text-align: left;
        }

        thead th {
            background: var(--md-table-header-bg);
            font-weight: 600;
        }

        tbody tr:nth-child(even) {
            background: var(--md-table-alt-row);
        }

        img {
            max-width: 100%;
        }

        input[type="checkbox"] {
            margin-right: 6px;
        }
        """;
}