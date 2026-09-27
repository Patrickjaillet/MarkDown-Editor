// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

namespace MarkDownEditor.Core.Services;

/// <summary>
/// Abstraction du chargement et de l'enregistrement des préférences
/// utilisateur (<see cref="AppSettings"/>) sur disque, dans le dossier de
/// l'application (portabilité stricte : jamais <c>%APPDATA%</c>, aucun
/// accès réseau).
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// Préférences actuellement chargées en mémoire. Toujours non nul
    /// après un appel à <see cref="LoadAsync"/> (des valeurs par défaut
    /// sont utilisées si aucun fichier local n'existe encore).
    /// </summary>
    AppSettings Current { get; }

    /// <summary>
    /// Charge les préférences depuis <c>config/settings.local.json</c> si
    /// ce fichier existe, sinon depuis <c>config/settings.default.json</c>,
    /// sinon utilise des valeurs par défaut codées en dur. Le résultat est
    /// exposé via <see cref="Current"/>.
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre l'état actuel de <see cref="Current"/> dans
    /// <c>config/settings.local.json</c>, à côté de l'exécutable.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);
}
