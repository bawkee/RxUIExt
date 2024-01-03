namespace RxUIExt.WinUI.SalaConverters;

using System;
using System.Windows;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// A catch-all null converter you can use when you're too lazy to use bool.
/// </summary>
public class NullConverter : IValueConverter
{
    /// <summary>
    /// Value to use when the actual value is null.
    /// </summary>
    public object Null { get; set; } = DependencyProperty.UnsetValue;

    /// <summary>
    /// Value to use when the actual value is not null.
    /// </summary>
    public object NotNull { get; set; } = DependencyProperty.UnsetValue;

    /// <inheritdoc />
    public virtual object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value is string stringValue && string.IsNullOrEmpty(stringValue))
            value = null; // Kind of sneaky but 99.99% of time you'll want this
        return value is not null ? NotNull : Null;
    }

    /// <inheritdoc />
    public virtual object ConvertBack(object value, Type targetType, object parameter, string culture) =>
        throw new InvalidOperationException();
}