// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Windows;
using MarkDownEditor.Core;
using MarkDownEditor.Core.Services;
using MarkDownEditor.Localization;
using MarkDownEditor.Rendering;
using MarkDownEditor.UI.Editing;
using MarkDownEditor.UI.Services;
using MarkDownEditor.UI.ViewModels;
using MarkDownEditor.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Alias pour éviter les conflits d'ambiguïté avec System.Windows.Shapes.Path
using File = System.IO.File;
using Path = System.IO.Path;

namespace MarkDownEditor.App;

/// <summary>
/// Point d'entrée de l'application. Met en place l'hôte générique
/// (<see cref="IHost"/>) pour l'injection de dépendances, entièrement
/// local : aucune configuration ni service ne requiert de connexion réseau.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>
    /// Dossier de l'exécutable, utilisé comme racine pour tous les assets
    /// portables (i18n, icônes, configuration utilisateur).
    /// </summary>
    private static string AppDirectory =>
        AppContext.BaseDirectory;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        await _host.StartAsync().ConfigureAwait(true);

        // Chargement des préférences persistées (config/settings.local.json,
        // jamais %APPDATA%) avant toute résolution de langue ou de thème,
        // afin que le choix explicite de l'utilisateur soit prioritaire sur
        // la détection automatique.
        var settingsService = _host.Services.GetRequiredService<IAppSettingsService>();
        await settingsService.LoadAsync().ConfigureAwait(true);

        var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
        await localizationService.SetLanguageAsync(ResolveStartupLanguage(settingsService.Current.Language)).ConfigureAwait(true);

        var themeService = _host.Services.GetRequiredService<IThemeService>();
        themeService.ApplyTheme(ResolveStartupTheme(settingsService.Current.Theme));

        var mainWindowViewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
        mainWindowViewModel.ApplyPersistedPreferences();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Ouverture directe par double-clic : le chemin du fichier .md est
        // passé en premier argument de ligne de commande par l'association
        // de fichier Windows (voir scripts/register-file-association.ps1).
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
        {
            await mainWindowViewModel.OpenFileAsync(args[1]).ConfigureAwait(true);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync().ConfigureAwait(true);
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        string i18nDirectory = Path.Combine(AppDirectory, "assets", "i18n");
        string configDirectory = Path.Combine(AppDirectory, "config");

        services.AddSingleton<ILocalizationService>(
            _ => new JsonLocalizationService(i18nDirectory));

        services.AddSingleton<IAppSettingsService>(
            _ => new AppSettingsService(configDirectory));

        services.AddSingleton<IMarkdownFileService, MarkdownFileService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IMarkdownRenderer, FlowDocumentMarkdownRenderer>();
        services.AddSingleton<IMarkdownOutlineService, MarkdownOutlineService>();
        services.AddSingleton<IHtmlExportService, HtmlExportService>();
        services.AddSingleton<IMarkdownHighlightingProvider, MarkdownHighlightingProvider>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        // Fenêtre « À propos » (voir ROADMAP.md, §4.6) : transitoire,
        // car ré-instanciée à chaque ouverture plutôt que conservée en
        // arrière-plan comme la fenêtre principale. Exposée à
        // MainWindow sous forme de fabrique (Func<AboutWindow>) plutôt
        // que résolue directement, afin que le code-behind n'ait pas à
        // dépendre du conteneur de dépendances lui-même.
        services.AddTransient<AboutWindowViewModel>();
        services.AddTransient<AboutWindow>();
        services.AddSingleton<Func<AboutWindow>>(
            provider => () => provider.GetRequiredService<AboutWindow>());
    }

    /// <summary>
    /// Détermine la langue de démarrage : la préférence explicite persistée
    /// (<c>"fr"</c> / <c>"en"</c>) est prioritaire ; en son absence ou avec
    /// la valeur <c>"auto"</c>, la langue système est détectée.
    /// </summary>
    /// <param name="persistedLanguage">Valeur de <see cref="AppSettings.Language"/>.</param>
    private static AppLanguage ResolveStartupLanguage(string persistedLanguage)
    {
        if (string.Equals(persistedLanguage, "fr", StringComparison.OrdinalIgnoreCase))
        {
            return AppLanguage.French;
        }

        if (string.Equals(persistedLanguage, "en", StringComparison.OrdinalIgnoreCase))
        {
            return AppLanguage.English;
        }

        // "auto" ou valeur inattendue : détection de la langue système.
        string systemLanguage = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return string.Equals(systemLanguage, "fr", StringComparison.OrdinalIgnoreCase)
            ? AppLanguage.French
            : AppLanguage.English;
    }

    /// <summary>
    /// Détermine le thème de démarrage à partir de la préférence persistée
    /// (<c>"light"</c> / <c>"dark"</c> / <c>"auto"</c>).
    /// </summary>
    /// <param name="persistedTheme">Valeur de <see cref="AppSettings.Theme"/>.</param>
    private static AppTheme ResolveStartupTheme(string persistedTheme)
    {
        return persistedTheme switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Auto,
        };
    }
}