<!-- SPDX-License-Identifier: GPL-3.0-or-later -->
<!-- Copyright (C) 2026 SANDEFJORD / Patrick JAILLET -->

# MarkDown Editor

Un éditeur et visualiseur Markdown natif Windows, entièrement
portable et hors-ligne, avec une expérience de lecture inspirée de la
typographie et de la mise en page soignées de Claude.ai.

English version: [README.md](README.md)

## Fonctionnalités

- **Mode Lecture** : rendu Markdown/GFM complet (titres, gras,
  italique, texte barré, listes ordonnées/non ordonnées, listes de
  tâches, citations, tableaux, liens, séparateurs), rendu nativement
  comme un `FlowDocument` WPF — aucun navigateur ni moteur HTML
  embarqué.
- **Thème clair / sombre**, bascule manuelle ou automatique selon le
  thème système Windows.
- **Ouverture directe des fichiers Markdown** par double-clic sur un
  fichier `.md`, une fois l'association de fichier enregistrée (voir
  plus bas).
- **Ouvrir / Enregistrer / Enregistrer sous**, avec les boîtes de
  dialogue natives Windows.
- **Interface bilingue** (français / anglais), détectée
  automatiquement depuis la langue système au premier lancement.
- **100 % portable** : un seul dossier autonome, aucun installeur,
  aucune écriture registre en dehors de l'association de fichier
  optionnelle.
- **100 % hors-ligne** : aucune requête réseau n'est jamais émise,
  aucune télémétrie.

## Prérequis

- Windows 10 ou Windows 11, 64 bits (x64).
- Aucune installation de .NET requise — l'application est publiée en
  mode autonome (self-contained).

## Démarrage

1. Téléchargez ou copiez le dossier `MarkDownEditor` où vous le
   souhaitez, y compris sur un support amovible.
2. Lancez `MarkDownEditor.exe`.
3. Utilisez **Ouvrir** pour charger un fichier `.md`, ou associez les
   fichiers `.md` à l'application (voir plus bas) pour les ouvrir
   directement depuis l'Explorateur Windows.

## Association de fichier (optionnelle)

MarkDown Editor ne modifie jamais le système sans votre consentement.
Pour associer les fichiers `.md` à l'application afin qu'un
double-clic les ouvre directement :

1. Ouvrez PowerShell dans le dossier `scripts`.
2. Exécutez `register-file-association.ps1`.

Cette opération n'écrit que dans votre ruche registre utilisateur
(`HKCU`) — aucun droit administrateur n'est requis, et rien n'est
installé à l'échelle du système. Pour annuler, exécutez
`unregister-file-association.ps1`.

## Compilation depuis les sources

Prérequis : SDK .NET 10, Windows 10/11 x64.

```powershell
scripts/build-portable.ps1
```

Cette commande produit une version portable et autonome dans `build/`
(ou le dossier de sortie configuré dans le script), prête à être
copiée où vous le souhaitez.

## Licence

MarkDown Editor est distribué sous licence GNU General Public License
v3.0 (GPL-3.0-or-later). Voir [LICENSE](../LICENSE) /
[COPYING](../COPYING) à la racine du dépôt, et
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) pour les licences des
dépendances tierces.

## Contact

- Site web : https://patrickjaillet.github.io/MarkDown-Editor
- E-mail : sandefjord.development@proton.me
