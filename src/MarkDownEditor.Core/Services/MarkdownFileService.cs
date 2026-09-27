// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Text;

namespace MarkDownEditor.Core.Services;

/// <summary>
/// Implémentation locale (système de fichiers) de <see cref="IMarkdownFileService"/>.
/// Toutes les opérations sont strictement locales : aucun accès réseau.
/// </summary>
public sealed class MarkdownFileService : IMarkdownFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <inheritdoc />
    public async Task<MarkdownDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Le fichier Markdown est introuvable : {filePath}",
                filePath);
        }

        string content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        return new MarkdownDocument(filePath, content);
    }

    /// <inheritdoc />
    public async Task SaveAsync(MarkdownDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(document.FilePath))
        {
            throw new InvalidOperationException(
                "Impossible d'enregistrer un document sans chemin de fichier associé. " +
                "Utiliser SaveAsAsync pour un premier enregistrement.");
        }

        await WriteToDiskAsync(document.FilePath, document.Content, cancellationToken).ConfigureAwait(false);
        document.MarkSaved(document.FilePath);
    }

    /// <inheritdoc />
    public async Task SaveAsAsync(
        MarkdownDocument document,
        string targetFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFilePath);

        await WriteToDiskAsync(targetFilePath, document.Content, cancellationToken).ConfigureAwait(false);
        document.MarkSaved(targetFilePath);
    }

    private static async Task WriteToDiskAsync(
        string filePath,
        string content,
        CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content, Utf8NoBom, cancellationToken).ConfigureAwait(false);
    }
}
