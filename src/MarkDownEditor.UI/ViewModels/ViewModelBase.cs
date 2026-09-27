// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using CommunityToolkit.Mvvm.ComponentModel;

namespace MarkDownEditor.UI.ViewModels;

/// <summary>
/// Classe de base commune à tous les ViewModels de MarkDown Editor.
/// S'appuie sur CommunityToolkit.Mvvm pour la notification de changement
/// de propriété (<see cref="ObservableObject"/>).
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
