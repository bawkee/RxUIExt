namespace RxUIExt.WinUI;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

public static class WindowExtensions
{
    public static AppWindow GetAppWindow(this Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }
}