// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core;

/// <summary>
/// Représente un document Markdown chargé en mémoire : son contenu brut,
/// son chemin sur disque (le cas échéant) et son état de modification.
/// </summary>
public sealed class MarkdownDocument
{
    /// <summary>
    /// Chemin absolu du fichier sur disque, ou <c>null</c> pour un document
    /// nouvellement créé et jamais encore enregistré.
    /// </summary>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Contenu Markdown brut du document.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Indique si le document contient des modifications non enregistrées
    /// depuis le dernier chargement ou la dernière sauvegarde.
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>
    /// Crée un nouveau document vide, non associé à un fichier.
    /// </summary>
    public MarkdownDocument()
    {
        FilePath = null;
        Content = string.Empty;
        IsDirty = false;
    }

    /// <summary>
    /// Crée un document à partir d'un contenu existant et d'un chemin de fichier.
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier source.</param>
    /// <param name="content">Contenu Markdown déjà chargé.</param>
    public MarkdownDocument(string filePath, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(content);

        FilePath = filePath;
        Content = content;
        IsDirty = false;
    }

    /// <summary>
    /// Met à jour le contenu du document et marque celui-ci comme modifié
    /// si le contenu a effectivement changé.
    /// </summary>
    /// <param name="newContent">Nouveau contenu Markdown.</param>
    public void UpdateContent(string newContent)
    {
        ArgumentNullException.ThrowIfNull(newContent);

        if (string.Equals(Content, newContent, StringComparison.Ordinal))
        {
            return;
        }

        Content = newContent;
        IsDirty = true;
    }

    /// <summary>
    /// Associe le document à un chemin de fichier et réinitialise l'état
    /// de modification (typiquement après un enregistrement réussi).
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier cible.</param>
    public void MarkSaved(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FilePath = filePath;
        IsDirty = false;
    }

    /// <summary>
    /// Nom d'affichage du document : nom de fichier si présent,
    /// sinon indicateur de document sans titre (à traduire côté UI).
    /// </summary>
    public string GetDisplayName()
    {
        return FilePath is null
            ? string.Empty
            : Path.GetFileName(FilePath);
    }
}
