namespace RxUIExt.WinUI.SalaConverters;

using Microsoft.UI.Xaml;

public class NullToVisibilityConverter : NullConverter
{
    public NullToVisibilityConverter()
    {
        Null = Visibility.Collapsed;
        NotNull = Visibility.Visible;
    }
}