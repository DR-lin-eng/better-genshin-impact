using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Windows.System;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Recognition.OCR;
using BetterGenshinImpact.Core.Script;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.AutoTrackPath;
using BetterGenshinImpact.GameTask.Common.Element.Assets;
using BetterGenshinImpact.GameTask.LogParse;
using BetterGenshinImpact.Helpers;
using BetterGenshinImpact.Helpers.Win32;
using BetterGenshinImpact.Model;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.Service.Notification;
using BetterGenshinImpact.View.Controls.Webview;
using BetterGenshinImpact.View.Converters;
using BetterGenshinImpact.View.Pages;
using BetterGenshinImpact.View.Windows;
using BetterGenshinImpact.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Meziantou.Framework.Win32;
using Microsoft.Extensions.Localization;
using Microsoft.Win32;
using Wpf.Ui;
using Wpf.Ui.Violeta.Controls;

namespace BetterGenshinImpact.ViewModel.Pages;

public partial class CommonSettingsPageViewModel : ViewModel
{
    private readonly INavigationService _navigationService;
    private readonly NotificationService _notificationService;
    private readonly TpConfig _tpConfig = TaskContext.Instance().Config.TpConfig;

    private string _selectedArea = string.Empty;
    private string _selectedCountry = string.Empty;
    
    [ObservableProperty] private List<string> _adventurersGuildCountry = [
        App.GetService<ILocalizationService>().GetString("country.none"),
        App.GetService<ILocalizationService>().GetString("country.fontaine"),
        App.GetService<ILocalizationService>().GetString("country.inazuma"),
        App.GetService<ILocalizationService>().GetString("country.liyue"),
        App.GetService<ILocalizationService>().GetString("country.mondstadt")
    ];

    /// <summary>
    /// ViewModel for managing language selection
    /// </summary>
    public LocalizationViewModel LocalizationViewModel { get; }

    public CommonSettingsPageViewModel(IConfigService configService, INavigationService navigationService,
        NotificationService notificationService, LocalizationViewModel localizationViewModel)
    {
        Config = configService.Get();
        _navigationService = navigationService;
        _notificationService = notificationService;
        LocalizationViewModel = localizationViewModel;
        
        InitializeCountries();
        InitializeMiyousheCookie();
        
        // Initialize localization after other components
        _ = InitializeLocalizationAsync();
    }

    /// <summary>
    /// Initializes the localization system
    /// </summary>
    private async Task InitializeLocalizationAsync()
    {
        try
        {
            await LocalizationViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            System.Diagnostics.Debug.WriteLine($"Failed to initialize localization: {ex.Message}");
        }
    }

    public AllConfig Config { get; set; }
    public ObservableCollection<string> CountryList { get; } = new();
    public ObservableCollection<string> Areas { get; } = new();
    
    public ObservableCollection<string> MapPathingTypes { get; } = ["SIFT", "TemplateMatch"];

    [ObservableProperty] private FrozenDictionary<string, string> _languageDict =
        new string[] { "zh-Hans", "zh-Hant", "en", "fr" }
            .ToFrozenDictionary(
                c => c,
                c =>
                {
                    CultureInfo.CurrentUICulture = new CultureInfo(c);
                    var stringLocalizer = App.GetService<IStringLocalizer<CultureInfoNameToKVPConverter>>() ??
                                          throw new NullReferenceException();
                    return stringLocalizer["简体中文"].ToString();
                }
            );

    public string SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            if (SetProperty(ref _selectedCountry, value))
            {
                UpdateAreas(value);
                SelectedArea = Areas.FirstOrDefault() ?? string.Empty;
            }
        }
    }

    public string SelectedArea
    {
        get => _selectedArea;
        set
        {
            if (SetProperty(ref _selectedArea, value))
            {
                UpdateRevivePoint(SelectedCountry, SelectedArea);
            }
        }
    }
    
    [RelayCommand]
    public void OnQuestionButtonOnClick()
    {
        //            Owner = this,
        WebpageWindow cookieWin = new()
        {
            Title = App.GetService<ILocalizationService>().GetString("window.logAnalysis"),
            Width = 800,
            Height = 600,

            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        cookieWin.NavigateToHtml(TravelsDiaryDetailManager.generHtmlMessage());
        cookieWin.Show();
    }
    private void InitializeMiyousheCookie()
    {
        OtherConfig.Miyoushe mcfg = TaskContext.Instance().Config.OtherConfig.MiyousheConfig;
        if (mcfg.Cookie == string.Empty&&
            mcfg.LogSyncCookie)
        {
            var config = LogParse.LoadConfig();
            mcfg.Cookie = config.Cookie;
        }
    }

    private void InitializeCountries()
    {
        var countries = MapLazyAssets.Instance.GoddessPositions.Values
            .OrderBy(g => int.TryParse(g.Id, out var id) ? id : int.MaxValue)
            .GroupBy(g => g.Country)
            .Select(grp => grp.Key);
        CountryList.Clear();
        foreach (var country in countries)
        {
            if (!string.IsNullOrEmpty(country))
            {
                CountryList.Add(country);
            }
        }

        _selectedCountry = _tpConfig.ReviveStatueOfTheSevenCountry;
        UpdateAreas(SelectedCountry);
        _selectedArea = _tpConfig.ReviveStatueOfTheSevenArea;
        UpdateRevivePoint(SelectedCountry, SelectedArea);
    }

    private void UpdateAreas(string country)
    {
        Areas.Clear();
        SelectedArea = string.Empty;
        if (string.IsNullOrEmpty(country)) return;

        var areas = MapLazyAssets.Instance.GoddessPositions.Values
            .Where(g => g.Country == country)
            .OrderBy(g => int.TryParse(g.Id, out var id) ? id : int.MaxValue)
            .GroupBy(g => g.Area)
            .Select(grp => grp.Key);
        foreach (var area in areas)
        {
            if (!string.IsNullOrEmpty(area))
            {
                Areas.Add(area);
            }
        }
    }

    // 当国家或区域改变时更新坐标
    private void UpdateRevivePoint(string country, string area)
    {
        if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(area)) return;

        var goddess = MapLazyAssets.Instance.GoddessPositions.Values
            .FirstOrDefault(g => g.Country == country && g.Area == area);
        if (goddess == null) return;
        _tpConfig.ReviveStatueOfTheSevenCountry = country;
        _tpConfig.ReviveStatueOfTheSevenArea = area;
        _tpConfig.ReviveStatueOfTheSevenPointX = goddess.X;
        _tpConfig.ReviveStatueOfTheSevenPointY = goddess.Y;
        _tpConfig.ReviveStatueOfTheSeven = goddess;
    }

    [RelayCommand]
    public void OnRefreshMaskSettings()
    {
        WeakReferenceMessenger.Default.Send(
            new PropertyChangedMessage<object>(this, "RefreshSettings", new object(), App.GetService<ILocalizationService>().GetString("message.recalculateControlPositions")));
    }

    [RelayCommand]
    private void OnSwitchMaskEnabled()
    {
        // if (Config.MaskWindowConfig.MaskEnabled)
        // {
        //     MaskWindow.Instance().Show();
        // }
        // else
        // {
        //     MaskWindow.Instance().Hide();
        // }
    }

    [RelayCommand]
    public void OnGoToHotKeyPage()
    {
        _navigationService.Navigate(typeof(HotKeyPage));
    }

    [RelayCommand]
    public void OnSwitchTakenScreenshotEnabled()
    {
    }

    [RelayCommand]
    public void OnGoToFolder()
    {
        var path = Global.Absolute(@"log\screenshot\");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Process.Start("explorer.exe", path);
    }

    [RelayCommand]
    public void OnGoToLogFolder()
    {
        var path = Global.Absolute(@"log");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Process.Start("explorer.exe", path);
    }

    [RelayCommand]
    private void ImportLocalScriptsRepoZip()
    {
        Directory.CreateDirectory(ScriptRepoUpdater.ReposPath);

        var dialog = new OpenFileDialog
        {
            Filter = "Zip Files (*.zip)|*.zip",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            var zipPath = dialog.FileName;
            // 删除旧文件夹
            if (Directory.Exists(ScriptRepoUpdater.CenterRepoPathOld))
            {
                DirectoryHelper.DeleteReadOnlyDirectory(ScriptRepoUpdater.CenterRepoPathOld);
            }

            ZipFile.ExtractToDirectory(zipPath, ScriptRepoUpdater.ReposPath, true);

            if (Directory.Exists(ScriptRepoUpdater.CenterRepoPathOld))
            {
                DirectoryHelper.CopyDirectory(ScriptRepoUpdater.CenterRepoPathOld, ScriptRepoUpdater.CenterRepoPath);
                MessageBox.Information(App.GetService<ILocalizationService>().GetString("message.scriptRepoImportSuccess"));
            }
            else
            {
                MessageBox.Error(App.GetService<ILocalizationService>().GetString("message.scriptRepoImportFailed"));
                DirectoryHelper.DeleteReadOnlyDirectory(ScriptRepoUpdater.ReposPath);
            }
        }
    }

    [RelayCommand]
    private void OpenAboutWindow()
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }

    [RelayCommand]
    private void OpenKeyBindingsWindow()
    {
        var keyBindingsWindow = KeyBindingsWindow.Instance;
        keyBindingsWindow.Owner = Application.Current.MainWindow;
        keyBindingsWindow.ShowDialog();
    }

    [RelayCommand]
    private async Task OnGameLangSelectionChanged(KeyValuePair<string, string> type)
    {
        await OcrFactory.ChangeCulture(type.Key);
    }

    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        await App.GetService<IUpdateService>()!.CheckUpdateAsync(new UpdateOption
        {
            Trigger = UpdateTrigger.Manual,
            Channel = UpdateChannel.Stable
        });
    }

    [RelayCommand]
    private async Task CheckUpdateAlphaAsync()
    {
        await App.GetService<IUpdateService>()!.CheckUpdateAsync(new UpdateOption
        {
            Trigger = UpdateTrigger.Manual,
            Channel = UpdateChannel.Alpha,
        });
    }

    [RelayCommand]
    private async Task GotoGithubActionAsync()
    {
        await Launcher.LaunchUriAsync(
            new Uri("https://github.com/babalae/better-genshin-impact/actions/workflows/publish.yml"));
    }
}