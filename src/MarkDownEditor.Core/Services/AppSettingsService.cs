// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarkDownEditor.Core.Services;

/// <summary>
/// Implémentation locale (système de fichiers, JSON) de
/// <see cref="IAppSettingsService"/>. Aucune opération n'accède au
/// réseau ni à <c>%APPDATA%</c> : tout reste dans le dossier
/// <c>config/</c> situé à côté de l'exécutable, pour une portabilité
/// stricte (utilisable depuis une clé USB sans laisser de traces).
/// </summary>
public sealed class AppSettingsService : IAppSettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    private readonly string _localSettingsFilePath;
    private readonly string _defaultSettingsFilePath;

    /// <inheritdoc />
    public AppSettings Current { get; private set; } = new();

    /// <summary>
    /// Crée le service de paramètres.
    /// </summary>
    /// <param name="configDirectory">
    /// Dossier de configuration absolu (typiquement <c>config/</c> à côté
    /// de l'exécutable).
    /// </param>
    public AppSettingsService(string configDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configDirectory);

        _localSettingsFilePath = Path.Combine(configDirectory, "settings.local.json");
        _defaultSettingsFilePath = Path.Combine(configDirectory, "settings.default.json");
    }

    /// <inheritdoc />
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        string? sourceFilePath = File.Exists(_localSettingsFilePath)
            ? _localSettingsFilePath
            : File.Exists(_defaultSettingsFilePath)
                ? _defaultSettingsFilePath
                : null;

        if (sourceFilePath is null)
        {
            Current = new AppSettings();
            return;
        }

        try
        {
            await using FileStream stream = File.OpenRead(sourceFilePath);
            AppSettings? loaded = await JsonSerializer
                .DeserializeAsync<AppSettings>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            Current = loaded ?? new AppSettings();
        }
        catch (JsonException)
        {
            // Fichier de paramètres corrompu ou illisible : on repart sur
            // des valeurs par défaut plutôt que de bloquer le démarrage
            // de l'application.
            Current = new AppSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(_localSettingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(_localSettingsFilePath);
        await JsonSerializer
            .SerializeAsync(stream, Current, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
