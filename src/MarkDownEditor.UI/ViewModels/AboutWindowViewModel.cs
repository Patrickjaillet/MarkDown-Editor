// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkDownEditor.Localization;

namespace MarkDownEditor.UI.ViewModels;

/// <summary>
/// ViewModel de la fenêtre « À propos » (voir ROADMAP.md, §4.6) :
/// affiche le nom du produit, le numéro de version, le copyright, la
/// licence, les coordonnées de contact et le lien du site officiel,
/// intégralement via <see cref="ILocalizationService"/> pour les
/// libellés et via les métadonnées d'assembly pour le numéro de
/// version, jamais codés en dur.
/// </summary>
public sealed partial class AboutWindowViewModel : ViewModelBase
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private string _titleText = string.Empty;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _versionText = string.Empty;

    [ObservableProperty]
    private string _copyrightText = string.Empty;

    [ObservableProperty]
    private string _licenseLabel = string.Empty;

    [ObservableProperty]
    private string _licenseName = string.Empty;

    [ObservableProperty]
    private string _licenseMention = string.Empty;

    [ObservableProperty]
    private string _contactLabel = string.Empty;

    [ObservableProperty]
    private string _contactEmail = string.Empty;

    [ObservableProperty]
    private string _websiteLabel = string.Empty;

    [ObservableProperty]
    private string _websiteUrl = string.Empty;

    [ObservableProperty]
    private string _thirdPartyNoticesLinkText = string.Empty;

    [ObservableProperty]
    private string _licenseFileLinkText = string.Empty;

    [ObservableProperty]
    private string _closeLabel = string.Empty;

    public AboutWindowViewModel(ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(localizationService);

        _localizationService = localizationService;

        RefreshLocalizedLabels();
    }

    /// <summary>
    /// Numéro de version résolu depuis les métadonnées de l'assembly
    /// d'entrée (renseignées par <c>Directory.Build.props</c>, propriété
    /// <c>&lt;Version&gt;</c>), jamais codé en dur dans le code source.
    /// </summary>
    private static string ResolveAssemblyVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        string? informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            // Ignore un éventuel suffixe de métadonnées de build (ex. "+abcdef").
            int plusIndex = informationalVersion.IndexOf('+');
            return plusIndex >= 0 ? informationalVersion[..plusIndex] : informationalVersion;
        }

        Version? version = assembly.GetName().Version;
        return version is null ? "0.0.0" : version.ToString(3);
    }

    private void RefreshLocalizedLabels()
    {
        TitleText = _localizationService.Translate("about.title");
        ProductName = _localizationService.Translate("about.productName");
        VersionText = _localizationService.Translate("about.versionLabel", ResolveAssemblyVersion());
        CopyrightText = _localizationService.Translate("about.copyright");
        LicenseLabel = _localizationService.Translate("about.licenseLabel");
        LicenseName = _localizationService.Translate("about.licenseName");
        LicenseMention = _localizationService.Translate("about.licenseMention");
        ContactLabel = _localizationService.Translate("about.contactLabel");
        ContactEmail = _localizationService.Translate("about.contactEmail");
        WebsiteLabel = _localizationService.Translate("about.websiteLabel");
        WebsiteUrl = _localizationService.Translate("about.websiteUrl");
        ThirdPartyNoticesLinkText = _localizationService.Translate("about.thirdPartyNoticesLink");
        LicenseFileLinkText = _localizationService.Translate("about.licenseFileLink");
        CloseLabel = _localizationService.Translate("dialogs.close");
    }
}
