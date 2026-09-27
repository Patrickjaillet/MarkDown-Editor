# SPDX-License-Identifier: GPL-3.0-or-later
# Copyright (C) 2026 SANDEFJORD / Patrick JAILLET
#
# Script de publication autonome et portable pour MarkDown Editor.
# Produit un dossier livrable win-x64 self-contained (exécutable unique + assets),
# 100 % hors-ligne, utilisable sans installeur ni droits administrateur.

#Requires -Version 5.1
$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Split-Path -Parent $ScriptRoot
$ProjectPath = Join-Path $RepoRoot "src\MarkDownEditor.App\MarkDownEditor.App.csproj"
$OutputDir = Join-Path $RepoRoot "dist\MarkDownEditor-portable-win-x64"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  MarkDown Editor - Publication autonome v1.0.0 (win-x64)   " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Projet introuvable : $ProjectPath"
    exit 1
}

# 1. Nettoyage de l'ancien dossier livrable
if (Test-Path $OutputDir) {
    Write-Host "Nettoyage de l'ancien dossier de publication..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}

# 2. Compilation et publication .NET 10
Write-Host "Compilation en mode portable (Release, self-contained, single-file, win-x64)..." -ForegroundColor Cyan

dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $OutputDir `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Error "Échec de la publication .NET (code d'erreur $LASTEXITCODE)."
    exit $LASTEXITCODE
}

# 3. Copie des fichiers légaux, de configuration et scripts d'association
Write-Host "Inclusion des ressources portables, configurations et documents légaux..." -ForegroundColor Cyan

# Configuration portable par défaut (espace corrigé sur Join-Path)
$ConfigDirDst = Join-Path $OutputDir "config"
$ConfigDefaultSrc = Join-Path $RepoRoot "config\settings.default.json"

if (Test-Path $ConfigDefaultSrc) {
    New-Item -ItemType Directory -Path $ConfigDirDst -Force | Out-Null
    Copy-Item -Path $ConfigDefaultSrc -Destination $ConfigDirDst -Force
}

# Scripts d'association de fichier
Copy-Item -Path (Join-Path $ScriptRoot "register-file-association.ps1") -Destination $OutputDir -Force
Copy-Item -Path (Join-Path $ScriptRoot "unregister-file-association.ps1") -Destination $OutputDir -Force

# Fichiers légaux et licences
$LicenseSrc = Join-Path $RepoRoot "LICENSE"
$CopyingSrc = Join-Path $RepoRoot "COPYING"
$ThirdPartyNoticesSrc = Join-Path $RepoRoot "docs\THIRD_PARTY_NOTICES.md"

if (Test-Path $LicenseSrc) { Copy-Item -Path $LicenseSrc -Destination $OutputDir -Force }
if (Test-Path $CopyingSrc) { Copy-Item -Path $CopyingSrc -Destination $OutputDir -Force }
if (Test-Path $ThirdPartyNoticesSrc) { Copy-Item -Path $ThirdPartyNoticesSrc -Destination $OutputDir -Force }

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host " Publication terminée avec succès !                         " -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host " Dossier livrable : $OutputDir" -ForegroundColor White
Write-Host " Ce dossier peut être copié/déplacé tel quel (clé USB, etc.)." -ForegroundColor Gray
Write-Host ""