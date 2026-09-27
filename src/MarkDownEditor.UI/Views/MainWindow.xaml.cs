// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.IO;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using MarkDownEditor.Core;
using MarkDownEditor.Localization;
using MarkDownEditor.UI.Editing;
using MarkDownEditor.UI.Services;
using MarkDownEditor.UI.ViewModels;
using Wpf.Ui.Appearance;

namespace MarkDownEditor.UI.Views;

/// <summary>
/// Fenêtre principale de MarkDown Editor. La logique applicative est
/// intégralement portée par <see cref="MainWindowViewModel"/> ; ce
/// code-behind se limite à l'initialisation WPF standard, au câblage de
/// l'éditeur AvalonEdit (non bindable nativement en MVVM pur), au
/// glisser-déposer de fichiers et à la confirmation de fermeture en cas
/// de modifications non enregistrées.
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ILocalizationService _localizationService;
    private readonly IMarkdownHighlightingProvider _highlightingProvider;
    private readonly IThemeService _themeService;
    private readonly Func<AboutWindow> _aboutWindowFactory;

    private bool _isSyncingFromViewModel;

    public MainWindow(
        MainWindowViewModel viewModel,
        ILocalizationService localizationService,
        IMarkdownHighlightingProvider highlightingProvider,
        IThemeService themeService,
        Func<AboutWindow> aboutWindowFactory)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(highlightingProvider);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(aboutWindowFactory);

        _viewModel = viewModel;
        _localizationService = localizationService;
        _highlightingProvider = highlightingProvider;
        _themeService = themeService;
        _aboutWindowFactory = aboutWindowFactory;

        InitializeComponent();
        DataContext = _viewModel;

        ApplicationThemeManager.Apply(this);

        MarkdownEditingAssistant.Attach(SourceEditor);
        SourceEditor.TextChanged += OnSourceEditorTextChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.NavigateToHeadingRequested += OnNavigateToHeadingRequested;
        _viewModel.AboutRequested += OnAboutRequested;

        _themeService.EffectiveThemeChanged += OnEffectiveThemeChanged;

        ApplyHighlighting();
        SyncEditorFromViewModel();
        ApplyViewMode();

        AllowDrop = true;
        PreviewDragEnter += OnPreviewDragOver;
        PreviewDragOver += OnPreviewDragOver;
        PreviewDragLeave += OnPreviewDragLeave;
        PreviewDrop += OnPreviewDrop;

        Closing += OnClosing;
        Closed += OnClosed;
    }

    /// <summary>
    /// Ouvre un fichier directement au démarrage (ouverture par
    /// double-clic ou association de fichier), après affichage de la
    /// fenêtre.
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier <c>.md</c> à ouvrir.</param>
    public async Task OpenInitialFileAsync(string filePath)
    {
        await _viewModel.OpenFileAsync(filePath).ConfigureAwait(true);
    }

    private void OnSourceEditorTextChanged(object? sender, EventArgs e)
    {
        if (_isSyncingFromViewModel)
        {
            return;
        }

        _viewModel.OnSourceTextEdited(SourceEditor.Text);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SourceText))
        {
            SyncEditorFromViewModel();
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.ViewMode))
        {
            ApplyViewMode();
        }
    }

    /// <summary>
    /// Ajuste la disposition des panneaux Édition et Lecture (colonnes de
    /// la grille de contenu) selon le mode d'affichage actif du
    /// ViewModel : un seul panneau visible en mode Lecture ou Édition,
    /// les deux côte à côte en vue partagée.
    /// </summary>
    private void ApplyViewMode()
    {
        switch (_viewModel.ViewMode)
        {
            case DocumentViewMode.Reading:
                EditorHost.Visibility = Visibility.Collapsed;
                ReadingHost.Visibility = Visibility.Visible;
                ContentGrid.ColumnDefinitions[0].Width = new GridLength(0);
                ContentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                break;

            case DocumentViewMode.Editing:
                EditorHost.Visibility = Visibility.Visible;
                ReadingHost.Visibility = Visibility.Collapsed;
                ContentGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                ContentGrid.ColumnDefinitions[1].Width = new GridLength(0);
                break;

            case DocumentViewMode.Split:
                EditorHost.Visibility = Visibility.Visible;
                ReadingHost.Visibility = Visibility.Visible;
                ContentGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                ContentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                break;
        }
    }

    /// <summary>
    /// Ouvre le fichier récent double-cliqué dans la barre latérale, après
    /// confirmation d'abandon des modifications non enregistrées le cas
    /// échéant.
    /// </summary>
    private async void OnRecentFileDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: RecentFileEntry entry })
        {
            return;
        }

        if (!await ConfirmDiscardUnsavedChangesIfAnyAsync().ConfigureAwait(true))
        {
            return;
        }

        await _viewModel.OpenFileAsync(entry.FilePath).ConfigureAwait(true);
    }

    /// <summary>
    /// Sélectionne l'entrée de table des matières cliquée dans la barre
    /// latérale et demande la navigation correspondante au ViewModel
    /// (voir ROADMAP.md, §6 Phase 3). Un simple clic gauche suffit,
    /// contrairement aux fichiers récents (double-clic), car il n'y a ici
    /// aucun risque de déclenchement accidentel d'une action destructrice.
    /// </summary>
    private void OnTableOfContentsEntryClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: TableOfContentsEntry entry })
        {
            return;
        }

        _viewModel.SelectTableOfContentsEntryCommand.Execute(entry);
    }

    private void SyncEditorFromViewModel()
    {
        if (string.Equals(SourceEditor.Text, _viewModel.SourceText, StringComparison.Ordinal))
        {
            return;
        }

        _isSyncingFromViewModel = true;
        int caretOffset = Math.Min(SourceEditor.CaretOffset, _viewModel.SourceText.Length);
        SourceEditor.Text = _viewModel.SourceText;
        SourceEditor.CaretOffset = caretOffset;
        _isSyncingFromViewModel = false;
    }

    private void OnEffectiveThemeChanged(object? sender, EventArgs e)
    {
        ApplyHighlighting();
    }

    /// <summary>
    /// Ouvre la fenêtre « À propos » (voir ROADMAP.md, §4.6), résolue
    /// via la fabrique injectée afin que chaque ouverture obtienne une
    /// instance fraîche (langue et thème toujours à jour) avec ses
    /// propres dépendances résolues par le conteneur.
    /// </summary>
    private void OnAboutRequested(object? sender, EventArgs e)
    {
        AboutWindow aboutWindow = _aboutWindowFactory();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    /// <summary>
    /// Fait défiler le panneau Lecture jusqu'au titre sélectionné dans la
    /// table des matières. Le nom d'ancre correspond exactement au
    /// <c>Name</c> posé sur le paragraphe du <c>FlowDocument</c> rendu
    /// (voir <see cref="MarkDownEditor.Rendering.FlowDocumentMarkdownRenderer"/>),
    /// ce qui permet une résolution directe par <c>FindName</c> sans
    /// dépendre d'un moteur HTML/navigateur.
    /// </summary>
    /// <param name="anchorName">Nom d'ancre du titre ciblé.</param>
    private void OnNavigateToHeadingRequested(object? sender, string anchorName)
    {
        if (_viewModel.RenderedDocument is null)
        {
            return;
        }

        // La navigation par table des matières n'a de sens que pour le
        // rendu Lecture ; en mode Édition seul, ce panneau est masqué,
        // mais on bascule tout de même en vue partagée pour que le
        // clic reste utile plutôt que silencieusement sans effet.
        if (_viewModel.ViewMode == DocumentViewMode.Editing)
        {
            _viewModel.SetViewModeCommand.Execute("Split");
        }

        if (_viewModel.RenderedDocument.FindName(anchorName) is not FrameworkContentElement target)
        {
            return;
        }

        target.BringIntoView();
    }

    private void ApplyHighlighting()
    {
        SourceEditor.SyntaxHighlighting = _highlightingProvider.GetDefinition(_themeService.CurrentRenderTheme);
    }

    private void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        bool isValidDrag = IsSingleMarkdownFileDrag(e);
        e.Effects = isValidDrag ? DragDropEffects.Copy : DragDropEffects.None;
        DragOverlay.Visibility = isValidDrag ? Visibility.Visible : Visibility.Collapsed;
        e.Handled = true;
    }

    private void OnPreviewDragLeave(object sender, DragEventArgs e)
    {
        DragOverlay.Visibility = Visibility.Collapsed;
        e.Handled = true;
    }

    private async void OnPreviewDrop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        DragOverlay.Visibility = Visibility.Collapsed;

        if (!IsSingleMarkdownFileDrag(e))
        {
            return;
        }

        string filePath = GetDroppedMarkdownFilePath(e)!;

        if (!await ConfirmDiscardUnsavedChangesIfAnyAsync().ConfigureAwait(true))
        {
            return;
        }

        await _viewModel.OpenFileAsync(filePath).ConfigureAwait(true);
    }

    private static bool IsSingleMarkdownFileDrag(DragEventArgs e)
    {
        return GetDroppedMarkdownFilePath(e) is not null;
    }

    private static string? GetDroppedMarkdownFilePath(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return null;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] { Length: 1 } paths)
        {
            return null;
        }

        string path = paths[0];
        return string.Equals(Path.GetExtension(path), ".md", StringComparison.OrdinalIgnoreCase)
            ? path
            : null;
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_viewModel.HasPendingChanges)
        {
            return;
        }

        // On bloque la fermeture le temps de demander confirmation de
        // façon asynchrone, puis on referme réellement la fenêtre si
        // l'utilisateur a choisi de poursuivre.
        e.Cancel = true;

        bool canClose = await ConfirmDiscardUnsavedChangesIfAnyAsync().ConfigureAwait(true);
        if (canClose)
        {
            Closing -= OnClosing;
            Close();
        }
    }

    /// <summary>
    /// Si le document actif possède des modifications non enregistrées,
    /// demande à l'utilisateur s'il souhaite les enregistrer, les
    /// abandonner, ou annuler l'opération en cours (fermeture ou
    /// ouverture d'un autre fichier par glisser-déposer).
    /// </summary>
    /// <returns>
    /// <c>true</c> si l'opération peut se poursuivre (modifications
    /// enregistrées ou explicitement abandonnées), <c>false</c> si
    /// l'utilisateur a annulé.
    /// </returns>
    private async Task<bool> ConfirmDiscardUnsavedChangesIfAnyAsync()
    {
        if (!_viewModel.HasPendingChanges)
        {
            return true;
        }

        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = _localizationService.Translate("dialogs.unsavedChangesTitle"),
            Content = _localizationService.Translate("dialogs.unsavedChangesMessage"),
            PrimaryButtonText = _localizationService.Translate("dialogs.save"),
            SecondaryButtonText = _localizationService.Translate("dialogs.discard"),
            CloseButtonText = _localizationService.Translate("dialogs.cancel"),
        };

        Wpf.Ui.Controls.MessageBoxResult result = await messageBox.ShowDialogAsync();

        switch (result)
        {
            case Wpf.Ui.Controls.MessageBoxResult.Primary:
                await _viewModel.SaveBeforeCloseAsync().ConfigureAwait(true);
                return !_viewModel.HasPendingChanges;

            case Wpf.Ui.Controls.MessageBoxResult.Secondary:
                return true;

            default:
                return false;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        SourceEditor.TextChanged -= OnSourceEditorTextChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _themeService.EffectiveThemeChanged -= OnEffectiveThemeChanged;
        PreviewDragEnter -= OnPreviewDragOver;
        PreviewDragOver -= OnPreviewDragOver;
        PreviewDragLeave -= OnPreviewDragLeave;
        PreviewDrop -= OnPreviewDrop;
        MarkdownEditingAssistant.Detach(SourceEditor);
    }
}
