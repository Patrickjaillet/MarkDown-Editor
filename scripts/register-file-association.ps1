# SPDX-License-Identifier: GPL-3.0-or-later
# Copyright (C) 2026 SANDEFJORD / Patrick JAILLET
#
# Associe les fichiers .md à MarkDown Editor pour l'utilisateur courant
# (HKEY_CURRENT_USER uniquement : aucune élévation administrateur requise).
# Ce script est un choix explicite de l'utilisateur ; il n'est jamais
# exécuté automatiquement par l'application elle-même.

#Requires -Version 5.1

$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$AppRoot = Split-Path -Parent $ScriptRoot
$ExecutablePath = Join-Path $AppRoot "MarkDownEditor.exe"

if (-not (Test-Path $ExecutablePath)) {
    Write-Error "Exécutable introuvable : $ExecutablePath`nCe script doit être exécuté depuis le dossier 'scripts' de l'application publiée."
    exit 1
}

$ProgId = "SANDEFJORD.MarkDownEditor.Document"
$ExtensionKey = "HKCU:\Software\Classes\.md"
$ProgIdKey = "HKCU:\Software\Classes\$ProgId"

Write-Host "Association du fichier .md à MarkDown Editor (utilisateur courant)..." -ForegroundColor Cyan
Write-Host "Exécutable : $ExecutablePath"

# Déclaration du ProgId
New-Item -Path $ProgIdKey -Force | Out-Null
Set-ItemProperty -Path $ProgIdKey -Name "(default)" -Value "Document MarkDown Editor"

$IconKey = Join-Path $ProgIdKey "DefaultIcon"
New-Item -Path $IconKey -Force | Out-Null
Set-ItemProperty -Path $IconKey -Name "(default)" -Value "$ExecutablePath,0"

$CommandKey = Join-Path $ProgIdKey "shell\open\command"
New-Item -Path $CommandKey -Force | Out-Null
Set-ItemProperty -Path $CommandKey -Name "(default)" -Value "`"$ExecutablePath`" `"%1`""

# Association de l'extension .md au ProgId
New-Item -Path $ExtensionKey -Force | Out-Null
Set-ItemProperty -Path $ExtensionKey -Name "(default)" -Value $ProgId

# Notifie l'explorateur Windows du changement d'association
Add-Type -Namespace Win32 -Name Shell32 -MemberDefinition @"
    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
"@
[Win32.Shell32]::SHChangeNotify(0x08000000, 0x0000, [IntPtr]::Zero, [IntPtr]::Zero)

Write-Host ""
Write-Host "Association terminée avec succès." -ForegroundColor Green
Write-Host "Les fichiers .md s'ouvriront désormais par double-clic avec MarkDown Editor."
Write-Host "Pour annuler cette association, exécutez : unregister-file-association.ps1"
