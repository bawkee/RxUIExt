namespace RxUIExt.WinUI;

using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;
using System.Reactive.Disposables;

/// <summary>
/// Provides extensions for showing a customized ContentDialog for various tasks.
/// </summary>
public static class DialogExtensions
{
    /// <summary>
    /// Provides a simple input box, similar to the one we had in VB6 days. Some things never change?
    /// </summary>
    public static async Task<string> ShowTextInputDialog(
        this UIElement parent,
        string prompt,
        string defaultValue = null,
        Action<TextInputDialogOptions> configure = null)
    {
        var inputTextBox = new TextBox
        {
            AcceptsReturn = false,
            Height = 32,
            Text = defaultValue
        };

        inputTextBox.SelectAll();

        var dialog = new ContentDialog
        {
            XamlRoot = parent.XamlRoot,
            Content = inputTextBox,
            Title = prompt,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "Ok",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var cd = new CompositeDisposable();

        var options = new TextInputDialogOptions
        {
            InputBox = inputTextBox,
            Dialog = dialog,
            Disposables = cd
        };

        configure?.Invoke(options);

        if (options.Validation != null)
            inputTextBox.Events()
                        .TextChanged
                        .Select(_ => inputTextBox.Text)
                        .StartWith(defaultValue)
                        .Select(v => options.Validation(v))
                        .Do(i => dialog.IsPrimaryButtonEnabled = i)
                        .SubscribeSafe()
                        .DisposeWith(cd);

        try
        {
            return await dialog.ShowAsync() == ContentDialogResult.Primary ? inputTextBox.Text : null;
        }
        finally
        {
            cd.Dispose();
        }
    }
}

/// <summary>
/// Options used for Input Dialog in <see cref="DialogExtensions"/>
/// </summary>
public class TextInputDialogOptions
{
    /// <summary>
    /// <see cref="TextBox"/> used for the input.
    /// </summary>
    public TextBox InputBox { get; internal init; }

    /// <summary>
    /// The <see cref="ContentDialog"/> itself.
    /// </summary>
    public ContentDialog Dialog { get; internal init; }

    /// <summary>
    /// If you have disposables of your own, add them here.
    /// </summary>
    public CompositeDisposable Disposables { get; internal init; }

    /// <summary>
    /// Validation logic for the Input Dialog.
    /// </summary>
    public Func<string, bool> Validation { get; set; }
}