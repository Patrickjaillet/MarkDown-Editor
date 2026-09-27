// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Collections.ObjectModel;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkDownEditor.Core;
using MarkDownEditor.Core.Services;
using MarkDownEditor.Localization;
using MarkDownEditor.Rendering;
using MarkDownEditor.UI.Services;
using Microsoft.Win32;

namespace MarkDownEditor.UI.ViewModels;

/// <summary>
/// ViewModel de la fenêtre principale. Pilote le cycle de vie du document
/// Markdown actif (ouverture, édition, enregistrement), sa présentation
/// (Lecture, Édition ou vue partagée), les fichiers récents, les
/// préférences persistées et la réaction aux changements de langue ou de
/// thème.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IMarkdownFileService _fileService;
    private readonly IThemeService _themeService;
    private readonly IMarkdownRenderer _markdownRenderer;
    private readonly IAppSettingsService _settingsService;
    private readonly IMarkdownOutlineService _outlineService;
    private readonly IHtmlExportService _htmlExportService;

    private MarkdownDocument _document;

    /// <summary>
    /// Vrai pendant que le ViewModel applique lui-même une modification au
    /// texte source (ex. ouverture d'un fichier) : évite de retraiter la
    /// mise à jour comme une frappe utilisateur.
    /// </summary>
    private bool _isApplyingExternalChange;

    [ObservableProperty]
    private string _windowTitle;

    [ObservableProperty]
    private FlowDocument? _renderedDocument;

    [ObservableProperty]
    private string _sourceText = string.Empty;

    [ObservableProperty]
    private DocumentViewMode _viewMode = DocumentViewMode.Reading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private int _wordCount;

    [ObservableProperty]
    private int _characterCount;

    [ObservableProperty]
    private int _estimatedReadingMinutes;

    [ObservableProperty]
    private string _openLabel = string.Empty;

    [ObservableProperty]
    private string _saveLabel = string.Empty;

    [ObservableProperty]
    private string _saveAsLabel = string.Empty;

    [ObservableProperty]
    private string _newLabel = string.Empty;

    [ObservableProperty]
    private string _toggleThemeLabel = string.Empty;

    [ObservableProperty]
    private string _readingModeLabel = string.Empty;

    [ObservableProperty]
    private string _editingModeLabel = string.Empty;

    [ObservableProperty]
    private string _splitViewLabel = string.Empty;

    [ObservableProperty]
    private string _recentFilesHeaderLabel = string.Empty;

    [ObservableProperty]
    private string _noRecentFilesLabel = string.Empty;

    [ObservableProperty]
    private string _wordCountLabel = string.Empty;

    [ObservableProperty]
    private string _characterCountLabel = string.Empty;

    [ObservableProperty]
    private string _readingTimeLabel = string.Empty;

    [ObservableProperty]
    private string _dragDropHintLabel = string.Empty;

    [ObservableProperty]
    private string _exportHtmlLabel = string.Empty;

    [ObservableProperty]
    private string _aboutLabel = string.Empty;

    [ObservableProperty]
    private string _recentFilesTabLabel = string.Empty;

    [ObservableProperty]
    private string _tableOfContentsTabLabel = string.Empty;

    [ObservableProperty]
    private string _noHeadingsLabel = string.Empty;

    [ObservableProperty]
    private string _newShortcutLabel = string.Empty;

    [ObservableProperty]
    private string _openShortcutLabel = string.Empty;

    [ObservableProperty]
    private string _saveShortcutLabel = string.Empty;

    [ObservableProperty]
    private string _saveAsShortcutLabel = string.Empty;

    [ObservableProperty]
    private string _exportHtmlShortcutLabel = string.Empty;

    [ObservableProperty]
    private string _toggleThemeShortcutLabel = string.Empty;

    [ObservableProperty]
    private string _aboutShortcutLabel = string.Empty;

    /// <summary>
    /// Liste observable des fichiers récents, pour affichage direct dans
    /// la barre latérale sans repasser par les préférences persistées.
    /// </summary>
    public ObservableCollection<RecentFileEntry> RecentFiles { get; } = [];

    /// <summary>
    /// Table des matières du document actif (voir ROADMAP.md, §6 Phase 3),
    /// régénérée à chaque rendu du document à partir de
    /// <see cref="IMarkdownOutlineService"/>. Affichée dans la barre
    /// latérale, la sélection d'une entrée fait défiler le panneau
    /// Lecture jusqu'au titre correspondant.
    /// </summary>
    public ObservableCollection<TableOfContentsEntry> TableOfContents { get; } = [];

    /// <summary>
    /// Levé lorsqu'une entrée de la table des matières est sélectionnée
    /// pour navigation ; la vue s'y abonne pour faire défiler le
    /// <c>FlowDocumentScrollViewer</c> jusqu'à l'ancre correspondante,
    /// une opération purement visuelle qui n'a pas sa place dans le
    /// ViewModel (pas d'accès direct au FlowDocument rendu côté vue).
    /// </summary>
    public event EventHandler<string>? NavigateToHeadingRequested;

    /// <summary>
    /// Levé lorsque l'utilisateur demande l'ouverture de la fenêtre
    /// « À propos » (voir ROADMAP.md, §4.6) ; la vue s'y abonne pour
    /// résoudre et afficher <c>AboutWindow</c> via l'injection de
    /// dépendances, la création d'une fenêtre n'ayant pas sa place dans
    /// le ViewModel.
    /// </summary>
    public event EventHandler? AboutRequested;

    public MainWindowViewModel(
        ILocalizationService localizationService,
        IMarkdownFileService fileService,
        IThemeService themeService,
        IMarkdownRenderer markdownRenderer,
        IAppSettingsService settingsService,
        IMarkdownOutlineService outlineService,
        IHtmlExportService htmlExportService)
    {
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(fileService);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(markdownRenderer);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(outlineService);
        ArgumentNullException.ThrowIfNull(htmlExportService);

        _localizationService = localizationService;
        _fileService = fileService;
        _themeService = themeService;
        _markdownRenderer = markdownRenderer;
        _settingsService = settingsService;
        _outlineService = outlineService;
        _htmlExportService = htmlExportService;

        _document = new MarkdownDocument();
        _windowTitle = _localizationService.Translate("app.title");

        _localizationService.LanguageChanged += OnLanguageChanged;
        _themeService.EffectiveThemeChanged += OnEffectiveThemeChanged;

        RefreshLocalizedLabels();
        RefreshRecentFilesCollection();
        RenderCurrentDocument();
        UpdateDocumentStatistics();
        UpdateTableOfContents();
    }

    /// <summary>
    /// Applique les préférences persistées (thème, mode d'affichage par
    /// défaut) chargées par <see cref="IAppSettingsService"/>. Appelé une
    /// fois au démarrage, après le chargement des paramètres.
    /// </summary>
    public void ApplyPersistedPreferences()
    {
        RefreshRecentFilesCollection();

        ViewMode = _settingsService.Current.ViewMode switch
        {
            "Editing" => DocumentViewMode.Editing,
            "Split" => DocumentViewMode.Split,
            _ => DocumentViewMode.Reading,
        };
    }

    /// <summary>
    /// Charge et affiche le fichier Markdown donné, typiquement reçu en
    /// argument de ligne de commande (double-clic) ou via glisser-déposer.
    /// </summary>
    /// <param name="filePath">Chemin absolu du fichier <c>.md</c> à ouvrir.</param>
    public async Task OpenFileAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            _document = await _fileService.LoadAsync(filePath).ConfigureAwait(true);
            HasUnsavedChanges = false;

            _isApplyingExternalChange = true;
            SourceText = _document.Content;
            _isApplyingExternalChange = false;

            RenderCurrentDocument();
            UpdateDocumentStatistics();
            UpdateWindowTitle();
            StatusMessage = string.Empty;

            _settingsService.Current.PushRecentFile(filePath);
            await _settingsService.SaveAsync().ConfigureAwait(true);
            RefreshRecentFilesCollection();
        }
        catch (Exception)
        {
            _settingsService.Current.RemoveRecentFile(filePath);
            await _settingsService.SaveAsync().ConfigureAwait(true);
            RefreshRecentFilesCollection();

            StatusMessage = _localizationService.Translate("errors.fileReadError", filePath);
        }
    }

    /// <summary>
    /// Réinitialise le document actif à un document vide et non associé à
    /// un fichier (commande « Nouveau »).
    /// </summary>
    [RelayCommand]
    private void New()
    {
        _document = new MarkdownDocument();
        HasUnsavedChanges = false;

        _isApplyingExternalChange = true;
        SourceText = string.Empty;
        _isApplyingExternalChange = false;

        RenderCurrentDocument();
        UpdateDocumentStatistics();
        UpdateWindowTitle();
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Markdown (*.md)|*.md|*.*|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog() == true)
        {
            await OpenFileAsync(dialog.FileName).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Ouvre un fichier récent depuis la barre latérale. Contourne
    /// silencieusement les entrées devenues invalides plutôt que de
    /// bloquer l'utilisateur : <see cref="OpenFileAsync"/> les retire
    /// déjà automatiquement de la liste en cas d'échec.
    /// </summary>
    /// <param name="entry">Entrée de fichier récent sélectionnée.</param>
    [RelayCommand]
    private async Task OpenRecentFileAsync(RecentFileEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        await OpenFileAsync(entry.FilePath).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_document.FilePath))
        {
            await SaveAsAsync().ConfigureAwait(true);
            return;
        }

        try
        {
            await _fileService.SaveAsync(_document).ConfigureAwait(true);
            HasUnsavedChanges = false;
            UpdateWindowTitle();
            StatusMessage = _localizationService.Translate("statusBar.savedAt", DateTime.Now.ToString("t"));
        }
        catch (Exception)
        {
            StatusMessage = _localizationService.Translate("errors.fileWriteError", _document.FilePath ?? string.Empty);
        }
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Markdown (*.md)|*.md",
            DefaultExt = ".md",
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _fileService.SaveAsAsync(_document, dialog.FileName).ConfigureAwait(true);
                HasUnsavedChanges = false;
                UpdateWindowTitle();
                StatusMessage = _localizationService.Translate("statusBar.savedAt", DateTime.Now.ToString("t"));

                _settingsService.Current.PushRecentFile(dialog.FileName);
                await _settingsService.SaveAsync().ConfigureAwait(true);
                RefreshRecentFilesCollection();
            }
            catch (Exception)
            {
                StatusMessage = _localizationService.Translate("errors.fileWriteError", dialog.FileName);
            }
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        AppTheme next = _themeService.IsDarkModeActive ? AppTheme.Light : AppTheme.Dark;
        _themeService.ApplyTheme(next);

        _settingsService.Current.Theme = next switch
        {
            AppTheme.Light => "light",
            AppTheme.Dark => "dark",
            _ => "auto",
        };

        _ = _settingsService.SaveAsync();
    }

    [RelayCommand]
    private void SetViewMode(string? mode)
    {
        ViewMode = mode switch
        {
            "Reading" => DocumentViewMode.Reading,
            "Editing" => DocumentViewMode.Editing,
            "Split" => DocumentViewMode.Split,
            _ => ViewMode,
        };

        _settingsService.Current.ViewMode = ViewMode.ToString();
        _ = _settingsService.SaveAsync();
    }

    /// <summary>
    /// Appelé par la vue à chaque frappe dans l'éditeur AvalonEdit (mode
    /// Édition ou vue partagée). Met à jour le document, marque l'état de
    /// modification et rafraîchit le rendu Lecture ainsi que les
    /// statistiques.
    /// </summary>
    /// <param name="newText">Contenu Markdown brut actuel de l'éditeur.</param>
    public void OnSourceTextEdited(string newText)
    {
        if (_isApplyingExternalChange)
        {
            return;
        }

        _document.UpdateContent(newText);
        HasUnsavedChanges = _document.IsDirty;

        RenderCurrentDocument();
        UpdateDocumentStatistics();
    }

    /// <summary>
    /// Indique si le document actif possède des modifications non
    /// enregistrées, pour permettre à la vue de demander confirmation
    /// avant de fermer ou d'ouvrir un autre fichier.
    /// </summary>
    public bool HasPendingChanges => _document.IsDirty;

    /// <summary>
    /// Enregistre le document actif s'il possède déjà un chemin de
    /// fichier associé, sinon ouvre la boîte de dialogue « Enregistrer
    /// sous ». Utilisé par la confirmation de fermeture.
    /// </summary>
    public async Task SaveBeforeCloseAsync()
    {
        await SaveAsync().ConfigureAwait(true);
    }

    private void OnLanguageChanged(object? sender, AppLanguage newLanguage)
    {
        UpdateWindowTitle();
        RefreshLocalizedLabels();
        UpdateDocumentStatistics();
    }

    private void RefreshLocalizedLabels()
    {
        OpenLabel = _localizationService.Translate("menu.file.open");
        SaveLabel = _localizationService.Translate("menu.file.save");
        SaveAsLabel = _localizationService.Translate("menu.file.saveAs");
        NewLabel = _localizationService.Translate("menu.file.new");
        ToggleThemeLabel = _localizationService.Translate("menu.view.toggleTheme");
        ReadingModeLabel = _localizationService.Translate("menu.view.readingMode");
        EditingModeLabel = _localizationService.Translate("menu.view.editingMode");
        SplitViewLabel = _localizationService.Translate("menu.view.splitView");
        RecentFilesHeaderLabel = _localizationService.Translate("sidebar.recentFiles");
        NoRecentFilesLabel = _localizationService.Translate("sidebar.noRecentFiles");
        DragDropHintLabel = _localizationService.Translate("dragDrop.overlayHint");
        ExportHtmlLabel = _localizationService.Translate("menu.file.exportHtml");
        AboutLabel = _localizationService.Translate("menu.help.about");
        RecentFilesTabLabel = _localizationService.Translate("sidebar.recentFilesTab");
        TableOfContentsTabLabel = _localizationService.Translate("sidebar.tableOfContentsTab");
        NoHeadingsLabel = _localizationService.Translate("sidebar.noHeadings");
        NewShortcutLabel = _localizationService.Translate("shortcuts.new");
        OpenShortcutLabel = _localizationService.Translate("shortcuts.open");
        SaveShortcutLabel = _localizationService.Translate("shortcuts.save");
        SaveAsShortcutLabel = _localizationService.Translate("shortcuts.saveAs");
        ExportHtmlShortcutLabel = _localizationService.Translate("shortcuts.exportHtml");
        ToggleThemeShortcutLabel = _localizationService.Translate("shortcuts.toggleTheme");
        AboutShortcutLabel = _localizationService.Translate("shortcuts.about");
    }

    private void RefreshRecentFilesCollection()
    {
        RecentFiles.Clear();
        foreach (string filePath in _settingsService.Current.RecentFiles)
        {
            RecentFiles.Add(new RecentFileEntry(filePath));
        }
    }

    private void OnEffectiveThemeChanged(object? sender, EventArgs e)
    {
        RenderCurrentDocument();
    }

    private void UpdateWindowTitle()
    {
        string appTitle = _localizationService.Translate("app.title");
        string documentName = _document.GetDisplayName();

        string baseTitle = string.IsNullOrEmpty(documentName)
            ? appTitle
            : $"{documentName} — {appTitle}";

        WindowTitle = HasUnsavedChanges ? $"● {baseTitle}" : baseTitle;
    }

    partial void OnHasUnsavedChangesChanged(bool value)
    {
        UpdateWindowTitle();
    }

    private void RenderCurrentDocument()
    {
        // Le rendu Lecture ne doit jamais faire échouer l'opération appelante
        // (ouverture de fichier, frappe clavier, bascule de thème…) : une
        // erreur de rendu est un problème d'affichage, pas un problème de
        // lecture ou d'enregistrement du fichier, et ne doit ni être confondue
        // avec l'une de ces erreurs, ni faire planter l'application.
        try
        {
            RenderedDocument = _markdownRenderer.Render(_document.Content, _themeService.CurrentRenderTheme);
        }
        catch (Exception)
        {
            RenderedDocument = null;
            StatusMessage = _localizationService.Translate("errors.renderError");
        }

        UpdateTableOfContents();
    }

    /// <summary>
    /// Régénère la table des matières à partir du contenu actif. Les
    /// noms d'ancre produits par <see cref="IMarkdownOutlineService"/>
    /// correspondent exactement à ceux posés sur le <c>FlowDocument</c>
    /// par <see cref="IMarkdownRenderer"/> (même logique de nommage
    /// partagée côté Rendering), ce qui permet à la vue de naviguer par
    /// simple <c>FindName</c> sans recalcul supplémentaire.
    /// </summary>
    private void UpdateTableOfContents()
    {
        TableOfContents.Clear();
        foreach (TableOfContentsEntry entry in _outlineService.ExtractHeadings(_document.Content))
        {
            TableOfContents.Add(entry);
        }
    }

    /// <summary>
    /// Demande la navigation vers un titre du document depuis la table
    /// des matières (voir <see cref="NavigateToHeadingRequested"/>).
    /// </summary>
    /// <param name="entry">Entrée de table des matières sélectionnée.</param>
    [RelayCommand]
    private void SelectTableOfContentsEntry(TableOfContentsEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        NavigateToHeadingRequested?.Invoke(this, entry.AnchorName);
    }

    /// <summary>
    /// Exporte le document actif en une page HTML autonome (voir
    /// ROADMAP.md, §4.4 et §6 Phase 3), via une boîte de dialogue
    /// « Enregistrer sous » proposant le même nom de base que le
    /// document, en <c>.html</c>.
    /// </summary>
    [RelayCommand]
    private async Task ExportHtmlAsync()
    {
        string suggestedFileName = string.IsNullOrEmpty(_document.FilePath)
            ? _localizationService.Translate("app.untitledDocument")
            : System.IO.Path.GetFileNameWithoutExtension(_document.FilePath);

        var dialog = new SaveFileDialog
        {
            Filter = "HTML (*.html)|*.html",
            DefaultExt = ".html",
            FileName = suggestedFileName,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            string documentTitle = string.IsNullOrEmpty(_document.FilePath)
                ? _localizationService.Translate("app.untitledDocument")
                : System.IO.Path.GetFileNameWithoutExtension(_document.FilePath);

            await _htmlExportService
                .ExportAsync(_document.Content, documentTitle, dialog.FileName)
                .ConfigureAwait(true);

            StatusMessage = _localizationService.Translate("statusBar.exportedAt", DateTime.Now.ToString("t"));
        }
        catch (Exception)
        {
            StatusMessage = _localizationService.Translate("errors.exportError", dialog.FileName);
        }
    }

    /// <summary>
    /// Demande l'ouverture de la fenêtre « À propos » (voir
    /// <see cref="AboutRequested"/>).
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        AboutRequested?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateDocumentStatistics()
    {
        string content = _document.Content;

        CharacterCount = content.Length;

        int words = content
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;
        WordCount = words;

        // Vitesse de lecture moyenne conventionnelle : 200 mots/minute,
        // avec un minimum d'une minute affichée dès qu'il y a du texte.
        const int averageWordsPerMinute = 200;
        EstimatedReadingMinutes = words == 0
            ? 0
            : Math.Max(1, (int)Math.Ceiling(words / (double)averageWordsPerMinute));

        WordCountLabel = _localizationService.Translate("statusBar.words", WordCount);
        CharacterCountLabel = _localizationService.Translate("statusBar.characters", CharacterCount);
        ReadingTimeLabel = _localizationService.Translate("statusBar.readingTime", EstimatedReadingMinutes);
    }
}

/// <summary>
/// Entrée affichable dans la liste des fichiers récents : associe le
/// chemin complet au nom de fichier utilisé pour l'affichage.
/// </summary>
/// <param name="FilePath">Chemin absolu complet du fichier.</param>
public sealed record RecentFileEntry(string FilePath)
{
    /// <summary>Nom de fichier seul, utilisé pour l'affichage en liste.</summary>
    public string FileName => System.IO.Path.GetFileName(FilePath);
}
