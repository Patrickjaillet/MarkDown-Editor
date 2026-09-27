# SPDX-License-Identifier: GPL-3.0-or-later
# Copyright (C) 2026 SANDEFJORD / Patrick JAILLET
#
# Supprime l'association des fichiers .md avec MarkDown Editor pour
# l'utilisateur courant (HKEY_CURRENT_USER uniquement).

#Requires -Version 5.1

$ErrorActionPreference = "Stop"

$ProgId = "SANDEFJORD.MarkDownEditor.Document"
$ExtensionKey = "HKCU:\Software\Classes\.md"
$ProgIdKey = "HKCU:\Software\Classes\$ProgId"

Write-Host "Suppression de l'association du fichier .md avec MarkDown Editor..." -ForegroundColor Cyan

if (Test-Path $ExtensionKey) {
    $currentValue = (Get-ItemProperty -Path $ExtensionKey -Name "(default)" -ErrorAction SilentlyContinue)."(default)"
    if ($currentValue -eq $ProgId) {
        Remove-Item -Path $ExtensionKey -Force -Recurse
        Write-Host "Clé d'extension .md supprimée."
    } else {
        Write-Host "L'extension .md est associée à une autre application ($currentValue) : aucune modification effectuée." -ForegroundColor Yellow
    }
} else {
    Write-Host "Aucune association .md trouvée pour l'utilisateur courant."
}

if (Test-Path $ProgIdKey) {
    Remove-Item -Path $ProgIdKey -Force -Recurse
    Write-Host "Définition du ProgId '$ProgId' supprimée."
}

Add-Type -Namespace Win32 -Name Shell32 -MemberDefinition @"
    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
"@
[Win32.Shell32]::SHChangeNotify(0x08000000, 0x0000, [IntPtr]::Zero, [IntPtr]::Zero)

Write-Host ""
Write-Host "Désassociation terminée." -ForegroundColor Green
