<!-- SPDX-License-Identifier: GPL-3.0-or-later -->
<!-- Copyright (C) 2026 SANDEFJORD / Patrick JAILLET -->

# MarkDown Editor

A native Windows Markdown editor and viewer, fully portable and
offline, with a reading experience inspired by Claude.ai's clean
typography and layout.

Version française : [README.fr.md](README.fr.md)

## Features

- **Reading mode**: full Markdown/GFM rendering (headings, bold,
  italic, strikethrough, ordered/unordered lists, task lists, block
  quotes, tables, links, thematic breaks) rendered natively as a WPF
  `FlowDocument` — no embedded browser or HTML engine.
- **Light / dark theme**, manual toggle or automatic, following the
  Windows system theme.
- **Open Markdown files directly** by double-clicking a `.md` file
  once the file association is registered (see below).
- **Open / Save / Save as**, with native Windows dialogs.
- **Bilingual interface** (English / French), auto-detected from the
  system language at first launch.
- **100% portable**: a single self-contained folder, no installer, no
  registry writes beyond the optional file association.
- **100% offline**: no network request is ever made, no telemetry.

## Requirements

- Windows 10 or Windows 11, 64-bit (x64).
- No .NET installation required — the application is published
  self-contained.

## Getting started

1. Download or copy the `MarkDownEditor` folder anywhere you like,
   including a removable drive.
2. Run `MarkDownEditor.exe`.
3. Use **Open** to load a `.md` file, or associate `.md` files with
   the application (see below) to open them directly from Windows
   Explorer.

## File association (optional)

MarkDown Editor never modifies the system without your consent. To
associate `.md` files with the application so double-clicking opens
them directly:

1. Open PowerShell in the `scripts` folder.
2. Run `register-file-association.ps1`.

This only writes to your user registry hive (`HKCU`) — no
administrator rights are required, and nothing is installed
system-wide. To undo this, run `unregister-file-association.ps1`.

## Building from source

Requirements: .NET 10 SDK, Windows 10/11 x64.

```powershell
scripts/build-portable.ps1
```

This produces a self-contained, portable build under `build/` (or the
output folder configured in the script), ready to copy anywhere.

## License

MarkDown Editor is distributed under the GNU General Public License
v3.0 (GPL-3.0-or-later). See [LICENSE](../LICENSE) /
[COPYING](../COPYING) at the repository root, and
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for third-party
dependency licenses.

## Contact

- Website: https://patrickjaillet.github.io/MarkDown-Editor
- Email: sandefjord.development@proton.me
