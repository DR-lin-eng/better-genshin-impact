using System.Diagnostics;
using System.Windows.Media;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.GameTask;
using Wpf.Ui.Controls;
using BetterGenshinImpact.Platform.Wine;

namespace BetterGenshinImpact.Helpers.Ui;

public class WindowHelper
{
    public static void TryApplySystemBackdrop(System.Windows.Window window)
    {
        var backdropType = TaskContext.Instance().Config.CommonConfig.CurrentBackdropType;

        // Wine 平台适配
        if (WinePlatformAddon.IsRunningOnWine)
        {
            try
            {
                ApplyBackdropToWindow(window, backdropType);
            }
            catch
            {
                Debug.WriteLine($"Failed to apply theme in Wine");
            }
        }
        else
        {
            ApplyBackdropToWindow(window, backdropType);
        }
    }

    /// <summary>
    /// 根据背景类型应用主题到指定窗口
    /// </summary>
    /// <param name="window">要应用主题的窗口</param>
    /// <param name="backdropType">背景类型</param>
    public static void ApplyBackdropToWindow(System.Windows.Window window, WindowBackdropType backdropType)
    {
        switch (backdropType)
        {
            case WindowBackdropType.None:
                window.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));
                break;
            case WindowBackdropType.Acrylic:
                window.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
                break;
            default:
                window.Background = new SolidColorBrush(Colors.Transparent);
                break;
        }

        WindowBackdrop.ApplyBackdrop(window, backdropType);
    }
}
