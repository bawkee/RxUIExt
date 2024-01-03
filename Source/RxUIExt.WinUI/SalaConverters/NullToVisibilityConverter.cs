namespace RxUIExt.WinUI.SalaConverters;

using Microsoft.UI.Xaml;

/// <summary>
/// Converts null values to Visibility.Collapsed and non-null values to Visibility.Visible.
/// </summary>
public class NullToVisibilityConverter : NullConverter
{
    /// <summary>
    /// Constructs the <c>NullToVisibilityConverter</c> class.
    /// </summary>
    public NullToVisibilityConverter()
    {
        Null = Visibility.Collapsed;
        NotNull = Visibility.Visible;
    }
}