// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core.Services;

/// <summary>
/// Abstraction des opérations de lecture/écriture de fichiers Markdown sur disque.
/// Aucune implémentation de ce contrat ne doit émettre de requête réseau :
/// le logiciel doit rester strictement hors-ligne.
/// </summary>
public interface IMarkdownFileService
{
    /// <summary>
    /// Charge un document Markdown depuis le chemin donné.
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier <c>.md</c> à charger.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le document chargé.</returns>
    Task<MarkdownDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre le document donné à l'emplacement <see cref="MarkdownDocument.FilePath"/>.
    /// </summary>
    /// <param name="document">Document à enregistrer ; doit déjà posséder un chemin de fichier.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    Task SaveAsync(MarkdownDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre le document donné à un nouvel emplacement et met à jour
    /// son chemin de fichier associé.
    /// </summary>
    /// <param name="document">Document à enregistrer.</param>
    /// <param name="targetFilePath">Nouveau chemin absolu de destination.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    Task SaveAsAsync(MarkdownDocument document, string targetFilePath, CancellationToken cancellationToken = default);
}
