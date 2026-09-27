// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using Markdig;

namespace MarkDownEditor.Rendering;

/// <summary>
/// Construit le pipeline Markdig partagé par l'ensemble du logiciel
/// (mode Lecture, export HTML, calcul de statistiques, etc.).
/// Centraliser la configuration ici garantit un rendu cohérent partout.
/// </summary>
public static class MarkdownPipelineFactory
{
    /// <summary>
    /// Crée un pipeline Markdig configuré avec les extensions GFM utiles
    /// (tableaux, texte barré, listes de tâches, liens automatiques),
    /// sans aucune extension nécessitant un accès réseau.
    /// </summary>
    public static MarkdownPipeline CreateDefault()
    {
        return new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseTaskLists()
            .UseAutoLinks()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();
    }
}
