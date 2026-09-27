// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright (C) 2026 SANDEFJORD / Patrick JAILLET

using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace MarkDownEditor.UI.Editing;

/// <summary>
/// Attache à un <see cref="TextEditor"/> AvalonEdit l'aide à la frappe du
/// mode Édition : auto-fermeture des paires de caractères
/// (<c>**</c>, <c>`</c>, <c>[]()</c>) et raccourcis de mise en forme
/// (Ctrl+B pour le gras, Ctrl+I pour l'italique).
/// </summary>
public static class MarkdownEditingAssistant
{
    /// <summary>
    /// Attache l'aide à la frappe à l'éditeur donné. Sans effet si déjà
    /// attachée à cette instance.
    /// </summary>
    /// <param name="editor">Éditeur AvalonEdit du mode Édition.</param>
    public static void Attach(TextEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        editor.TextArea.TextEntering += OnTextEntering;
        editor.TextArea.PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <summary>
    /// Détache l'aide à la frappe précédemment attachée par
    /// <see cref="Attach"/>. À appeler lors de la libération de la vue
    /// pour éviter toute fuite d'abonnement aux événements.
    /// </summary>
    /// <param name="editor">Éditeur AvalonEdit du mode Édition.</param>
    public static void Detach(TextEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        editor.TextArea.TextEntering -= OnTextEntering;
        editor.TextArea.PreviewKeyDown -= OnPreviewKeyDown;
    }

    private static void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextArea textArea)
        {
            return;
        }

        bool isControlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        if (!isControlDown)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.B:
                WrapSelectionOrInsert(textArea, "**", "**");
                e.Handled = true;
                break;

            case Key.I:
                WrapSelectionOrInsert(textArea, "_", "_");
                e.Handled = true;
                break;
        }
    }

    private static void OnTextEntering(object? sender, TextCompositionEventArgs e)
    {
        if (sender is not TextArea textArea || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        // L'auto-fermeture ne s'applique qu'à la frappe d'un seul
        // caractère sans sélection active : avec une sélection, on laisse
        // le raccourci Ctrl+B/Ctrl+I (WrapSelectionOrInsert) s'en charger,
        // et une frappe multi-caractères provient typiquement d'une IME
        // qu'il ne faut pas perturber.
        if (e.Text.Length != 1 || !textArea.Selection.IsEmpty)
        {
            return;
        }

        char typedChar = e.Text[0];
        string? closingText = typedChar switch
        {
            '`' => "`",
            '(' => ")",
            '[' => "]",
            _ => null,
        };

        if (closingText is null)
        {
            return;
        }

        // Pour '[', on insère la paire complète "[]()" en une seule fois
        // afin de couvrir le cas d'usage "lien Markdown" mentionné dans
        // le ROADMAP, puis on place le curseur entre crochets.
        if (typedChar == '[')
        {
            e.Handled = true;
            int offset = textArea.Caret.Offset;
            textArea.Document.Insert(offset, "[]()");
            textArea.Caret.Offset = offset + 1;
            return;
        }

        e.Handled = true;
        int caretOffset = textArea.Caret.Offset;
        textArea.Document.Insert(caretOffset, typedChar + closingText);
        textArea.Caret.Offset = caretOffset + 1;
    }

    private static void WrapSelectionOrInsert(TextArea textArea, string prefix, string suffix)
    {
        TextDocument document = textArea.Document;

        if (!textArea.Selection.IsEmpty)
        {
            int start = textArea.Selection.SurroundingSegment?.Offset ?? textArea.Caret.Offset;
            int length = textArea.Selection.SurroundingSegment?.Length ?? 0;
            string selectedText = document.GetText(start, length);

            document.Replace(start, length, prefix + selectedText + suffix);
            textArea.Selection = Selection.Create(textArea, start + prefix.Length, start + prefix.Length + selectedText.Length);
            return;
        }

        int caretOffset = textArea.Caret.Offset;
        document.Insert(caretOffset, prefix + suffix);
        textArea.Caret.Offset = caretOffset + prefix.Length;
    }
}
