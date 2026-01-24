using BetterGenshinImpact.Helpers.DpiAwareness;
using BetterGenshinImpact.Helpers.Ui;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.View.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vanara.PInvoke;
using Wpf.Ui.Controls;

namespace BetterGenshinImpact.View;

public class CapturableWindow
{
    public IntPtr Handle { get; }
    public string Name { get; }
    public string ProcessName { get; }
    public ImageSource? Icon { get; }

    public CapturableWindow(IntPtr handle, string name, string processName, ImageSource? icon)
    {
        Handle = handle;
        Name = name;
        ProcessName = processName;
        Icon = icon;
    }
}

public partial class PickerWindow : FluentWindow
{
    private bool _isSelected;
    private readonly bool _captureTest;

    private const User32.WindowStylesEx IgnoreExStyle = User32.WindowStylesEx.WS_EX_TOOLWINDOW |
                                                        User32.WindowStylesEx.WS_EX_NOREDIRECTIONBITMAP |
                                                        User32.WindowStylesEx.WS_EX_LAYERED;

    public PickerWindow(bool captureTest = false)
    {
        InitializeComponent();
        this.InitializeDpiAwareness();
        SourceInitialized += OnSourceInitialized;
        MouseLeftButtonDown += PickerWindow_MouseLeftButtonDown;
        _captureTest = captureTest;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        WindowHelper.TryApplySystemBackdrop(this);
    }

    public bool PickCaptureTarget(IntPtr hWnd, out IntPtr pickedWindow)
    {
        new WindowInteropHelper(this).Owner = hWnd;
        ShowDialog();
        if (!_isSelected)
        {
            pickedWindow = IntPtr.Zero;
            return false;
        }
        pickedWindow = ((CapturableWindow?)WindowList.SelectedItem)?.Handle ?? IntPtr.Zero;
        return true;
    }

    private void FindWindows()
    {
        var wih = new WindowInteropHelper(this);
        var windows = new List<CapturableWindow>();

        User32.EnumWindows((hWnd, lParam) =>
        {
            if (!User32.IsWindowVisible(hWnd) || wih.Handle == hWnd)
                return true;

            var exStyle = User32.GetWindowLong<User32.WindowStylesEx>(hWnd, User32.WindowLongFlags.GWL_EXSTYLE);
            if ((exStyle & IgnoreExStyle) != 0)
                return true;

            var title = new StringBuilder(1024);
            _ = User32.GetWindowText(hWnd, title, title.Capacity);
            if (string.IsNullOrWhiteSpace(title.ToString()))
                return true;

            _ = User32.GetWindowThreadProcessId(hWnd, out var processId);
            var process = Process.GetProcessById((int)processId);

            // Get the window icon if available.
            var icon = GetWindowIcon((IntPtr)hWnd);

            windows.Add(new CapturableWindow((IntPtr)hWnd, title.ToString(), process.ProcessName, icon));

            return true;
        }, IntPtr.Zero);

        var sortedWindows = windows.OrderByDescending(IsGenshinWindow)
            .ThenByDescending(x => x.Handle).ToList();

        WindowList.ItemsSource = sortedWindows;
    }

    private ImageSource? GetWindowIcon(IntPtr hWnd)
    {
        try
        {
            const int ICON_BIG = 1;
            const int ICON_SMALL = 0;
            const int GCL_HICON = -14;

            var iconHandle = User32.SendMessage(hWnd, User32.WindowMessage.WM_GETICON, (IntPtr)ICON_BIG, IntPtr.Zero);

            if (iconHandle == IntPtr.Zero)
            {
                iconHandle = User32.SendMessage(hWnd, User32.WindowMessage.WM_GETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
            }

            if (iconHandle == IntPtr.Zero)
            {
                iconHandle = User32.GetClassLong(hWnd, GCL_HICON);
            }

            if (iconHandle != IntPtr.Zero)
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    iconHandle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{App.GetService<ILocalizationService>().GetString("log.error.failedToGetWindowIcon")}: {ex.Message}");
        }

        return null;
    }

    private static bool IsGenshinWindow(CapturableWindow window)
    {
        return window is
            { Name: "原神", ProcessName: "YuanShen" } or
            { Name: "云·原神", ProcessName: "Genshin Impact Cloud Game" } or
            { Name: "Genshin Impact", ProcessName: "GenshinImpact" } or
            { Name: "Genshin Impact · Cloud", ProcessName: "Genshin Impact Cloud" };
    }

    private static bool AskIsThisGenshinImpact(CapturableWindow window)
    {
        var localizationService = App.GetService<ILocalizationService>();
        var res = ThemedMessageBox.Question(
            $"""
            {localizationService.GetString("picker.notGenshinConfirmation")}

            {localizationService.GetString("picker.currentSelectedWindow")}: {window.Name} ({window.ProcessName})
            """,
            localizationService.GetString("picker.confirmSelection"),
            MessageBoxButton.YesNo,
            MessageBoxResult.No
        );
        return res == MessageBoxResult.Yes;
    }

    private void WindowsOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WindowList.SelectedItem is not CapturableWindow selectedWindow)
            return;

        if (!_captureTest && !IsGenshinWindow(selectedWindow))
        {
            if (!AskIsThisGenshinImpact(selectedWindow))
            {
                return;
            }
        }
        _isSelected = true;
        Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        FindWindows();
    }

    private void PickerWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void FluentWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _isSelected = false;
            Close();
        }
    }

    private void cancelButton_Click(object sender, RoutedEventArgs e)
    {
        _isSelected = false;
        Close();
    }
}
