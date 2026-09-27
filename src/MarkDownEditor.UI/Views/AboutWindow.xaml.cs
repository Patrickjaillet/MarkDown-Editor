// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using MarkDownEditor.UI.ViewModels;
using Wpf.Ui.Appearance;

namespace MarkDownEditor.UI.Views;

/// <summary>
/// Fenêtre « À propos » (voir ROADMAP.md, §4.6) : affiche le nom du
/// produit, la version, le copyright, la licence, le contact et le
/// lien du site officiel, avec un accès rapide au texte complet de la
/// licence (<c>LICENSE</c>) et aux notices tierces
/// (<c>THIRD_PARTY_NOTICES.md</c>). Ouvre ces documents et le site
/// officiel via le shell du système d'exploitation (navigateur ou
/// application associée par défaut de l'utilisateur), jamais via une
/// requête réseau initiée par l'application elle-même.
/// </summary>
public partial class AboutWindow
{
    public AboutWindow(AboutWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;

        ApplicationThemeManager.Apply(this);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnWebsiteHyperlinkClick(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Documents.Hyperlink hyperlink || hyperlink.NavigateUri is null)
        {
            return;
        }

        OpenWithShell(hyperlink.NavigateUri.ToString());
    }

    private void OnOpenLicenseClick(object sender, RoutedEventArgs e)
    {
        string licensePath = Path.Combine(AppContext.BaseDirectory, "LICENSE");
        OpenWithShell(licensePath);
    }

    private void OnOpenThirdPartyNoticesClick(object sender, RoutedEventArgs e)
    {
        string noticesPath = Path.Combine(AppContext.BaseDirectory, "THIRD_PARTY_NOTICES.md");
        OpenWithShell(noticesPath);
    }

    /// <summary>
    /// Ouvre un chemin local (fichier livré à côté de l'exécutable) ou
    /// une URL via l'association par défaut du système d'exploitation
    /// (navigateur, lecteur Markdown, etc.). L'application elle-même
    /// n'émet aucune requête réseau : c'est le shell de l'utilisateur,
    /// hors du processus de MarkDown Editor, qui prend en charge
    /// l'ouverture, exactement comme un double-clic dans l'explorateur
    /// de fichiers.
    /// </summary>
    /// <param name="target">Chemin de fichier local ou URL à ouvrir.</param>
    private static void OpenWithShell(string target)
    {
        try
        {
            var startInfo = new ProcessStartInfo(target)
            {
                UseShellExecute = true,
            };
            Process.Start(startInfo);
        }
        catch (Exception)
        {
            // Absence d'application associée, fichier introuvable, etc. :
            // on n'interrompt pas l'utilisation de la fenêtre « À propos »
            // pour une action secondaire qui a échoué silencieusement.
        }
    }
}
