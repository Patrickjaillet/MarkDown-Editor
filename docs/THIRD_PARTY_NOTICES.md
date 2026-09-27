<!-- SPDX-License-Identifier: GPL-3.0-or-later -->
<!-- Copyright (C) 2026 SANDEFJORD / Patrick JAILLET -->

# Third-Party Notices / Notices tierces

This document lists every third-party dependency used by MarkDown
Editor, its license, and confirmation of compatibility with
GPL-3.0-or-later, the license under which MarkDown Editor is
distributed.

Ce document recense chaque dépendance tierce utilisée par MarkDown
Editor, sa licence, et une confirmation de compatibilité avec la
GPL-3.0-or-later, licence sous laquelle MarkDown Editor est distribué.

---

## NuGet dependencies / Dépendances NuGet

| Name / Nom | Version | License / Licence | License text / Texte de licence | GPL-3.0 compatible? |
|---|---|---|---|---|
| Markdig | 0.37.0 | BSD-3-Clause | https://github.com/xoofx/markdig/blob/master/license.txt | Yes — permissive license, compatible with GPL-3.0 as a linked dependency. |
| WPF-UI | 4.3.0 | MIT | https://github.com/lepoco/wpfui/blob/main/LICENSE | Yes — permissive license, compatible with GPL-3.0 as a linked dependency. |
| CommunityToolkit.Mvvm | 8.4.0 | MIT | https://github.com/CommunityToolkit/dotnet/blob/main/License.md | Yes — permissive license, compatible with GPL-3.0 as a linked dependency. |
| Microsoft.Extensions.Hosting | 9.0.0 | MIT | https://github.com/dotnet/runtime/blob/main/LICENSE.TXT | Yes — permissive license, compatible with GPL-3.0 as a linked dependency. |
| System.Text.Json | 9.0.0 | MIT | https://github.com/dotnet/runtime/blob/main/LICENSE.TXT | Yes — permissive license, compatible with GPL-3.0 as a linked dependency. |

No dependency in this project requires network access, telemetry, or
an online service at any point (build or runtime).

Aucune dépendance de ce projet ne requiert d'accès réseau, de
télémétrie ni de service en ligne, que ce soit à la compilation ou à
l'exécution.

---

## .NET / Runtime

MarkDown Editor targets `net10.0-windows` and is published
self-contained (see `src/MarkDownEditor.App/MarkDownEditor.App.csproj`).
The .NET runtime itself is distributed by Microsoft under the MIT
license. See https://github.com/dotnet/runtime/blob/main/LICENSE.TXT.

MarkDown Editor cible `net10.0-windows` et est publié en mode
self-contained (voir `src/MarkDownEditor.App/MarkDownEditor.App.csproj`).
Le runtime .NET lui-même est distribué par Microsoft sous licence MIT.
Voir https://github.com/dotnet/runtime/blob/main/LICENSE.TXT.

---

## Fonts / Polices

The application uses system-installed fonts as its primary typefaces
(`Segoe UI Variable`, `Segoe UI`, `Calibri` for body text; `Cascadia
Code`, `Consolas`, `Courier New` for monospace text — see
`src/MarkDownEditor.Rendering/Styling/MarkdownRenderTheme.cs`). These
are Windows system fonts and are not redistributed with the
application; no font file is embedded in `assets/fonts/`.

L'application utilise les polices installées par le système comme
typographies principales (`Segoe UI Variable`, `Segoe UI`, `Calibri`
pour le texte courant ; `Cascadia Code`, `Consolas`, `Courier New`
pour le texte à chasse fixe — voir
`src/MarkDownEditor.Rendering/Styling/MarkdownRenderTheme.cs`). Ce
sont des polices système Windows, non redistribuées avec
l'application ; aucun fichier de police n'est embarqué dans
`assets/fonts/`.

If any third-party font file is added to `assets/fonts/` in the
future, its license must be verified for GPL-3.0 compatibility and
redistribution rights, and recorded in this file before inclusion.

Si un fichier de police tiers est ajouté à `assets/fonts/` à l'avenir,
sa licence devra être vérifiée pour sa compatibilité GPL-3.0 et ses
droits de redistribution, puis consignée dans ce fichier avant
intégration.

---

## Icons / Icônes

Application icon (`assets/icons/app.ico`) is an original asset created
for this project; no third-party icon set is embedded.

L'icône de l'application (`assets/icons/app.ico`) est un asset
original créé pour ce projet ; aucun jeu d'icônes tiers n'est
embarqué.

The WPF-UI library (see above) ships its own Fluent System Icons font
used for in-app UI icons (buttons, menus). This font is part of the
WPF-UI NuGet package and covered by the WPF-UI MIT license referenced
above.

La bibliothèque WPF-UI (voir ci-dessus) embarque sa propre police
d'icônes Fluent System Icons, utilisée pour les icônes de l'interface
(boutons, menus). Cette police fait partie du paquet NuGet WPF-UI et
est couverte par la licence MIT de WPF-UI référencée ci-dessus.

---

## Maintenance

This file must be updated whenever a new third-party dependency
(NuGet package, font, icon set, or any other embedded asset) is added
to the project, before the dependency is merged. Each new entry
requires: name, version, license, link to license text, and an
explicit compatibility statement with GPL-3.0-or-later.

Ce fichier doit être mis à jour à chaque ajout d'une nouvelle
dépendance tierce (paquet NuGet, police, jeu d'icônes, ou tout autre
asset embarqué) au projet, avant que la dépendance ne soit intégrée.
Chaque nouvelle entrée requiert : nom, version, licence, lien vers le
texte de licence, et une déclaration explicite de compatibilité avec
la GPL-3.0-or-later.
