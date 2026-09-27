// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using MarkDownEditor.Core;
using MarkDownEditor.Rendering.Styling;
using Wpf.Ui.Appearance;

namespace MarkDownEditor.UI.Services;

/// <summary>
/// Implémentation de <see cref="IThemeService"/> qui s'appuie sur
/// <see cref="ApplicationThemeManager"/> (WPF-UI) pour l'apparence des
/// fenêtres et contrôles, et sur <see cref="MarkdownRenderTheme"/> pour
/// la palette de rendu du document.
/// </summary>
public sealed class ThemeService : IThemeService
{
    /// <inheritdoc />
    public AppTheme RequestedTheme { get; private set; } = AppTheme.Auto;

    /// <inheritdoc />
    public bool IsDarkModeActive { get; private set; }

    /// <inheritdoc />
    public MarkdownRenderTheme CurrentRenderTheme { get; private set; } = MarkdownRenderTheme.Light;

    /// <inheritdoc />
    public event EventHandler? EffectiveThemeChanged;

    public ThemeService()
    {
        // Réagit aux changements de thème système lorsque le mode Auto est actif.
        ApplicationThemeManager.Changed += OnSystemThemeChanged;
    }

    /// <inheritdoc />
    public void ApplyTheme(AppTheme theme)
    {
        RequestedTheme = theme;

        ApplicationTheme themeToApply = theme switch
        {
            AppTheme.Light => ApplicationTheme.Light,
            AppTheme.Dark => ApplicationTheme.Dark,
            AppTheme.Auto => ApplicationThemeManager.GetSystemTheme() switch
            {
                SystemTheme.Dark => ApplicationTheme.Dark,
                _ => ApplicationTheme.Light,
            },
            _ => ApplicationTheme.Light,
        };

        ApplicationThemeManager.Apply(themeToApply);

        UpdateEffectiveTheme(themeToApply);
    }

    private void OnSystemThemeChanged(ApplicationTheme currentApplicationTheme, System.Windows.Media.Color systemAccent)
    {
        if (RequestedTheme == AppTheme.Auto)
        {
            UpdateEffectiveTheme(currentApplicationTheme);
        }
    }

    private void UpdateEffectiveTheme(ApplicationTheme effectiveTheme)
    {
        bool isDark = effectiveTheme == ApplicationTheme.Dark;

        if (isDark == IsDarkModeActive && CurrentRenderTheme is not null)
        {
            return;
        }

        IsDarkModeActive = isDark;
        CurrentRenderTheme = isDark ? MarkdownRenderTheme.Dark : MarkdownRenderTheme.Light;

        EffectiveThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
