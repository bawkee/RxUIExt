namespace RxUIExt.WinUI;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

/// <summary>
/// Extensions that help with WinUI <see cref="Window"/> type.
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// Get main WinUI app window.
    /// </summary>
    public static AppWindow GetAppWindow(this Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }
}