// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Localization;

/// <summary>
/// Service de traduction de l'interface, entièrement piloté par des
/// fichiers JSON embarqués dans le dossier de l'application (aucun
/// appel réseau, aucune dépendance à un service de traduction en ligne).
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Langue actuellement active pour l'interface.
    /// </summary>
    AppLanguage CurrentLanguage { get; }

    /// <summary>
    /// Se déclenche après un changement réussi de langue via <see cref="SetLanguageAsync"/>.
    /// </summary>
    event EventHandler<AppLanguage>? LanguageChanged;

    /// <summary>
    /// Charge le fichier de traduction correspondant à la langue donnée
    /// et la définit comme langue active.
    /// </summary>
    /// <param name="language">Langue à activer.</param>
    Task SetLanguageAsync(AppLanguage language);

    /// <summary>
    /// Résout la traduction associée à une clé hiérarchique
    /// (ex. <c>"menu.file.open"</c>) dans la langue actuellement active.
    /// </summary>
    /// <param name="key">Clé de traduction, avec segments séparés par des points.</param>
    /// <returns>
    /// Le texte traduit, ou la clé elle-même entre crochets si aucune
    /// traduction n'est trouvée (ex. <c>"[menu.file.open]"</c>), afin de
    /// rendre tout oubli de traduction immédiatement visible en interface.
    /// </returns>
    string Translate(string key);

    /// <summary>
    /// Résout la traduction associée à une clé et l'interpole avec les
    /// arguments positionnels fournis (placeholders <c>{0}</c>, <c>{1}</c>, …).
    /// </summary>
    /// <param name="key">Clé de traduction.</param>
    /// <param name="args">Arguments à substituer dans le texte traduit.</param>
    string Translate(string key, params object[] args);
}
