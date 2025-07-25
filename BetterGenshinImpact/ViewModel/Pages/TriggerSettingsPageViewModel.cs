﻿using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.GameTask.AutoPick;
using BetterGenshinImpact.GameTask.AutoSkip.Assets;
using BetterGenshinImpact.GameTask.AutoSkip.Model;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.View.Pages;
using BetterGenshinImpact.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BetterGenshinImpact.GameTask;
using Wpf.Ui;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace BetterGenshinImpact.ViewModel.Pages;

public partial class TriggerSettingsPageViewModel : ViewModel
{
    [ObservableProperty] private string[] _clickChatOptionNames = [
        App.GetService<ILocalizationService>().GetString("chat.selectFirstOption"),
        App.GetService<ILocalizationService>().GetString("chat.selectRandomOption"),
        App.GetService<ILocalizationService>().GetString("chat.selectLastOption"),
        App.GetService<ILocalizationService>().GetString("chat.noSelection")
    ];

    [ObservableProperty] private string[] _selectChatOptionTypeNames = [SelectChatOptionTypes.UseMouse, SelectChatOptionTypes.UseInteractionKey];

    [ObservableProperty] private string[] _pickOcrEngineNames = [PickOcrEngineEnum.Paddle.ToString(), PickOcrEngineEnum.Yap.ToString()];

    [ObservableProperty] private List<string> _pickButtonNames;

    public AllConfig Config { get; set; }

    private readonly INavigationService _navigationService;

    [ObservableProperty] private List<string> _hangoutBranches;

    public TriggerSettingsPageViewModel(IConfigService configService, INavigationService navigationService)
    {
        Config = configService.Get();
        _navigationService = navigationService;
        _hangoutBranches = HangoutConfig.Instance.HangoutOptionsTitleList;

        _pickButtonNames = new List<string> { "F", "E", "G" };
        if (!string.IsNullOrEmpty(Config.AutoPickConfig.PickKey)
            && Config.AutoPickConfig.PickKey.Length == 1
            && char.IsUpper(Config.AutoPickConfig.PickKey[0])
            && !_pickButtonNames.Contains(Config.AutoPickConfig.PickKey))
        {
            _pickButtonNames.Add(Config.AutoPickConfig.PickKey);
        }
    }

    [RelayCommand]
    private void OnEditBlacklist()
    {
        var path = @"User\pick_black_lists.txt";
        var text = Global.ReadAllTextIfExist(path);
        if (string.IsNullOrEmpty(text))
        {
            text = "";
        }

        var multilineTextBox = new TextBox
        {
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            Height = 340,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            PlaceholderText = App.GetService<ILocalizationService>().GetString("dialog.blacklistPlaceholder")
        };
        var localizationService = App.GetService<ILocalizationService>();
        var p = new PromptDialog(
            localizationService.GetString("dialog.blacklistConfig"),
            localizationService.GetString("dialog.blacklistTitle"),
            multilineTextBox,
            text);
        p.Height = 500;
        p.ShowDialog();
        if (p.DialogResult == true)
        {
            File.WriteAllText(Global.Absolute(path), multilineTextBox.Text);
            GameTaskManager.RefreshTriggerConfigs();
        }
    }

    [RelayCommand]
    private void OnEditWhitelist()
    {
        var path = @"User\pick_white_lists.txt";
        var text = Global.ReadAllTextIfExist(path);
        if (string.IsNullOrEmpty(text))
        {
            text = "";
        }

        var multilineTextBox = new TextBox
        {
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            Height = 340,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            PlaceholderText = App.GetService<ILocalizationService>().GetString("dialog.whitelistPlaceholder")
        };
        var localizationService = App.GetService<ILocalizationService>();
        var p = new PromptDialog(
            localizationService.GetString("dialog.whitelistConfig"),
            localizationService.GetString("dialog.whitelistTitle"),
            multilineTextBox,
            text);
        p.Height = 500;
        p.ShowDialog();
        if (p.DialogResult == true)
        {
            File.WriteAllText(Global.Absolute(path), multilineTextBox.Text);
            GameTaskManager.RefreshTriggerConfigs();
        }
    }

    // [RelayCommand]
    // private void OnOpenReExploreCharacterBox(object sender)
    // {
    //     var str = PromptDialog.Prompt("请使用派遣界面展示的角色名，英文逗号分割，从左往右优先级依次降低。\n示例：菲谢尔,班尼特,夜兰,申鹤,久岐忍",
    //         "派遣角色优先级配置", Config.AutoSkipConfig.AutoReExploreCharacter);
    //     Config.AutoSkipConfig.AutoReExploreCharacter = str.Replace("，", ",").Replace(" ", "");
    // }

    [RelayCommand]
    public void OnGoToHotKeyPage()
    {
        _navigationService.Navigate(typeof(HotKeyPage));
    }
}