namespace RxUIExt.WinUI.SalaConverters;

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// You can use this bool converter for anything, colors, brushes, visibility, opacity, thickness, etc. 
/// </summary>
public class BoolConverter : IValueConverter
{
    /// <summary>
    /// Value to use when actual value is True.
    /// </summary>
    public virtual object True { get; set; } = DependencyProperty.UnsetValue;

    /// <summary>
    /// Value to use when actual value is False.
    /// </summary>
    public virtual object Default { get; set; } = DependencyProperty.UnsetValue;

    /// <summary>
    /// Whether to invert the actual value.
    /// </summary>
    public bool Invert { get; set; }

    /// <inheritdoc />
    public virtual object Convert(object value, Type targetType, object parameter, string culture) =>
        value is true ? Invert ? Default : True : Invert ? True : Default;

    /// <inheritdoc />
    public virtual object ConvertBack(object value, Type targetType, object parameter, string culture) =>
        value.Equals(Invert ? Default : True);
}

/// <summary>
/// A generic base class for converters you'd be using very often.
/// </summary>
public abstract class BoolConverter<T> : IValueConverter
{
    /// <summary>
    /// Constructs this <c>BoolConverter</c> from the given values.
    /// </summary>
    /// <param name="trueValue">Value to use when actual value is True.</param>
    /// <param name="falseValue">Value to use when actual value is False.</param>
    protected BoolConverter(T trueValue = default, T falseValue = default)
    {
        True = trueValue;
        False = falseValue;
    }

    /// <summary>
    /// Value to use when actual value is True.
    /// </summary>
    public T True { get; set; }

    /// <summary>
    /// Value to use when actual value is False.
    /// </summary>
    public T False { get; set; }

    /// <summary>
    /// Whether to invert the actual value.
    /// </summary>
    public bool Invert { get; set; }

    /// <inheritdoc />
    public virtual object Convert(object value, Type targetType, object parameter, string culture) =>
        value is true ? Invert ? False : True : Invert ? True : False;

    /// <inheritdoc />
    public virtual object ConvertBack(object value, Type targetType, object parameter, string culture) =>
        value is T tvalue && EqualityComparer<T>.Default.Equals(tvalue, Invert ? False : True);
}
