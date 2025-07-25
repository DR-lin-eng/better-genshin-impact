﻿using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Model;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.GameTask;
using Wpf.Ui.Controls;
using BetterGenshinImpact.Genshin.Settings;
using CommunityToolkit.Mvvm.Input;
using static Vanara.PInvoke.User32;

namespace BetterGenshinImpact.ViewModel.Pages;

public partial class KeyBindingsSettingsPageViewModel : ViewModel
{
    private readonly ILogger<KeyBindingsSettingsPageViewModel> _logger;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// 配置文件
    /// </summary>
    private readonly KeyBindingsConfig _config;

    [ObservableProperty]
    private ObservableCollection<KeyBindingSettingModel> _keyBindingSettingModels = [];

    public KeyBindingsSettingsPageViewModel(IConfigService configService, ILogger<KeyBindingsSettingsPageViewModel> logger, ILocalizationService localizationService)
    {
        _logger = logger;
        _localizationService = localizationService;

        // 获取本模块的配置文件
        _config = configService.Get().KeyBindingsConfig;

        BuildKeyBindingsList();

        var list = GetAllNonDirectoryKeyBinding(KeyBindingSettingModels);
        foreach (var keyConfig in list)
        {
            keyConfig.PropertyChanged += (sender, e) =>
            {
                if (sender is KeyBindingSettingModel model)
                {
                    // 使用反射更新配置文件
                    if (e.PropertyName == nameof(model.KeyValue))
                    {
                        Debug.WriteLine(_localizationService.GetString("keyBindings.bindingChanged", model.ActionName, model.KeyValue));

                        var pi = _config.GetType().GetProperty(model.ConfigPropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (pi != null && pi.CanWrite)
                        {
                            pi.SetValue(_config, model.KeyValue, null);
                        }
                    }
                }
            };
        }
    }


    public static List<KeyBindingSettingModel> GetAllNonDirectoryKeyBinding(IEnumerable<KeyBindingSettingModel> modelList)
    {
        var list = new List<KeyBindingSettingModel>();
        foreach (var keyBindingSettingModel in modelList)
        {
            if (!keyBindingSettingModel.IsDirectory)
            {
                list.Add(keyBindingSettingModel);
            }

            list.AddRange(GetAllNonDirectoryChildren(keyBindingSettingModel));
        }

        return list;
    }

    public static List<KeyBindingSettingModel> GetAllNonDirectoryChildren(KeyBindingSettingModel model)
    {
        var result = new List<KeyBindingSettingModel>();

        if (model.Children.Count == 0)
        {
            return result;
        }

        foreach (var child in model.Children)
        {
            if (!child.IsDirectory)
            {
                result.Add(child);
            }

            // 递归调用以获取子节点中的非目录对象
            result.AddRange(GetAllNonDirectoryChildren(child));
        }

        return result;
    }

    /// <summary>
    /// 构造按键绑定列表
    /// </summary>
    private void BuildKeyBindingsList()
    {
        // 动作按键
        var actionDirectory = new KeyBindingSettingModel(_localizationService.GetString("keyBindings.actions"));

        // 菜单按键
        var menuDirectory = new KeyBindingSettingModel(_localizationService.GetString("keyBindings.menus"));

        // 添加动作按键的二级菜单
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.moveForward"),
                    nameof(_config.MoveForward),
                    _config.MoveForward
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.moveBackward"),
                    nameof(_config.MoveBackward),
                    _config.MoveBackward
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.moveLeft"),
                    nameof(_config.MoveLeft),
                    _config.MoveLeft
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.moveRight"),
                    nameof(_config.MoveRight),
                    _config.MoveRight
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchToWalkOrRun"),
                    nameof(_config.SwitchToWalkOrRun),
                    _config.SwitchToWalkOrRun
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.normalAttack"),
                    nameof(_config.NormalAttack),
                    _config.NormalAttack
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.elementalSkill"),
                    nameof(_config.ElementalSkill),
                    _config.ElementalSkill
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.elementalBurst"),
                    nameof(_config.ElementalBurst),
                    _config.ElementalBurst
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.sprintKeyboard"),
                    nameof(_config.SprintKeyboard),
                    _config.SprintKeyboard
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.sprintMouse"),
                    nameof(_config.SprintMouse),
                    _config.SprintMouse
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchAimingMode"),
                    nameof(_config.SwitchAimingMode),
                    _config.SwitchAimingMode
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.jump"),
                    nameof(_config.Jump),
                    _config.Jump
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.drop"),
                    nameof(_config.Drop),
                    _config.Drop
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.pickUpOrInteract"),
                    nameof(_config.PickUpOrInteract),
                    _config.PickUpOrInteract
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.quickUseGadget"),
                    nameof(_config.QuickUseGadget),
                    _config.QuickUseGadget
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.interactionInSomeMode"),
                    nameof(_config.InteractionInSomeMode),
                    _config.InteractionInSomeMode
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.questNavigation"),
                    nameof(_config.QuestNavigation),
                    _config.QuestNavigation
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.abandonChallenge"),
                    nameof(_config.AbandonChallenge),
                    _config.AbandonChallenge
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchMember1"),
                    nameof(_config.SwitchMember1),
                    _config.SwitchMember1
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchMember2"),
                    nameof(_config.SwitchMember2),
                    _config.SwitchMember2
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchMember3"),
                    nameof(_config.SwitchMember3),
                    _config.SwitchMember3
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchMember4"),
                    nameof(_config.SwitchMember4),
                    _config.SwitchMember4
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.switchMember5"),
                    nameof(_config.SwitchMember5),
                    _config.SwitchMember5
                ));
        actionDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.shortcutWheel"),
                    nameof(_config.ShortcutWheel),
                    _config.ShortcutWheel
                ));

        // 添加菜单按键的二级菜单
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openInventory"),
                    nameof(_config.OpenInventory),
                    _config.OpenInventory
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openCharacterScreen"),
                    nameof(_config.OpenCharacterScreen),
                    _config.OpenCharacterScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openMap"),
                    nameof(_config.OpenMap),
                    _config.OpenMap
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openPaimonMenu"),
                    nameof(_config.OpenPaimonMenu),
                    _config.OpenPaimonMenu
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openAdventurerHandbook"),
                    nameof(_config.OpenAdventurerHandbook),
                    _config.OpenAdventurerHandbook
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openCoOpScreen"),
                    nameof(_config.OpenCoOpScreen),
                    _config.OpenCoOpScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openWishScreen"),
                    nameof(_config.OpenWishScreen),
                    _config.OpenWishScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openBattlePassScreen"),
                    nameof(_config.OpenBattlePassScreen),
                    _config.OpenBattlePassScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openTheEventsMenu"),
                    nameof(_config.OpenTheEventsMenu),
                    _config.OpenTheEventsMenu
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openTheSettingsMenu"),
                    nameof(_config.OpenTheSettingsMenu),
                    _config.OpenTheSettingsMenu
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openTheFurnishingScreen"),
                    nameof(_config.OpenTheFurnishingScreen),
                    _config.OpenTheFurnishingScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openStellarReunion"),
                    nameof(_config.OpenStellarReunion),
                    _config.OpenStellarReunion
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openQuestMenu"),
                    nameof(_config.OpenQuestMenu),
                    _config.OpenQuestMenu
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openNotificationDetails"),
                    nameof(_config.OpenNotificationDetails),
                    _config.OpenNotificationDetails
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openChatScreen"),
                    nameof(_config.OpenChatScreen),
                    _config.OpenChatScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openSpecialEnvironmentInformation"),
                    nameof(_config.OpenSpecialEnvironmentInformation),
                    _config.OpenSpecialEnvironmentInformation
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.checkTutorialDetails"),
                    nameof(_config.CheckTutorialDetails),
                    _config.CheckTutorialDetails
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.elementalSight"),
                    nameof(_config.ElementalSight),
                    _config.ElementalSight
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.showCursor"),
                    nameof(_config.ShowCursor),
                    _config.ShowCursor
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openPartySetupScreen"),
                    nameof(_config.OpenPartySetupScreen),
                    _config.OpenPartySetupScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.openFriendsScreen"),
                    nameof(_config.OpenFriendsScreen),
                    _config.OpenFriendsScreen
                ));
        menuDirectory.Children.Add(new KeyBindingSettingModel(
                    _localizationService.GetString("keyBindings.hideUI"),
                    nameof(_config.HideUI),
                    _config.HideUI
                ));

        KeyBindingSettingModels.Add(actionDirectory);
        KeyBindingSettingModels.Add(menuDirectory);
    }

    /// <summary>
    /// 从注册表中读取按键
    /// </summary>
    [RelayCommand]
    private void FetchFromRegistry()
    {
        // 读取注册表
        SettingsContainer settings = new();
        settings.FromReg();

        var keySetting = settings.OverrideController?.KeyboardMap?.ActionElementMap;
        if (keySetting != null)
        {
            foreach (var item in keySetting)
            {
                if (item.ElementIdentifierId is { } keyId)
                {
                    // 跳过无法识别的按键
                    if (keyId.ToName() == "Unknown" || keyId.ToName() == "None")
                    {
                        continue;
                    }
                    var key = KeyIdConverter.FromVK(keyId.ToVK());
                    switch (item.ActionId)
                    {
                        case ActionId.MoveForwardShow:
                            _config.MoveForward = key;
                            break;
                        case ActionId.MoveBackShow:
                            _config.MoveBackward = key;
                            break;
                        case ActionId.MoveLeftShow:
                            _config.MoveLeft = key;
                            break;
                        case ActionId.MoveRightShow:
                            _config.MoveRight = key;
                            break;
                        case ActionId.WalkRun:
                            _config.SwitchToWalkOrRun = key;
                            break;
                        case ActionId.Attack:
                            _config.NormalAttack = key;
                            break;
                        case ActionId.ElementalSkill:
                            _config.ElementalSkill = key;
                            break;
                        case ActionId.ElementalBurst:
                            _config.ElementalBurst = key;
                            break;
                        case ActionId.Sprint:
                            if (key >= KeyId.MouseLeftButton && key <= KeyId.MouseSideButton2)
                            {
                                _config.SprintMouse = key;
                            }
                            else
                            {
                                _config.SprintKeyboard = key;
                            }
                            break;
                        case ActionId.AimMode:
                            _config.SwitchAimingMode = key;
                            break;
                        case ActionId.Jump:
                            _config.Jump = key;
                            break;
                        case ActionId.CancelClimb:
                            _config.Drop = key;
                            break;
                        // TODO: 拾取&交互可能不正确
                        case ActionId.UseTalk:
                            _config.PickUpOrInteract = key;
                            break;
                        case ActionId.Gadget:
                            _config.QuickUseGadget = key;
                            break;
                        case ActionId.InteractionSomeModes:
                            _config.InteractionInSomeMode = key;
                            break;
                        case ActionId.Navigation:
                            _config.QuestNavigation = key;
                            break;
                        case ActionId.AbandonChallenge:
                            _config.AbandonChallenge = key;
                            break;
                        case ActionId.Char1:
                            _config.SwitchMember1 = key;
                            break;
                        case ActionId.Char2:
                            _config.SwitchMember2 = key;
                            break;
                        case ActionId.Char3:
                            _config.SwitchMember3 = key;
                            break;
                        case ActionId.Char4:
                            _config.SwitchMember4 = key;
                            break;
                        case ActionId.Char5:
                            _config.SwitchMember5 = key;
                            break;
                        case ActionId.QuickWheel:
                            _config.ShortcutWheel = key;
                            break;
                        case ActionId.Inventory:
                            _config.OpenInventory = key;
                            break;
                        case ActionId.CharacterList:
                            _config.OpenCharacterScreen = key;
                            break;
                        case ActionId.Map:
                            _config.OpenMap = key;
                            break;
                        // TODO: 派蒙界面可能不正确
                        case ActionId.MenuGamepad:
                            _config.OpenPaimonMenu = key;
                            break;
                        case ActionId.AdventurerHandbook:
                            _config.OpenAdventurerHandbook = key;
                            break;
                        case ActionId.CoOp:
                            _config.OpenCoOpScreen = key;
                            break;
                        case ActionId.Wish:
                            _config.OpenWishScreen = key;
                            break;
                        case ActionId.BattlePass:
                            _config.OpenBattlePassScreen = key;
                            break;
                        case ActionId.Events:
                            _config.OpenTheEventsMenu = key;
                            break;
                        case ActionId.PotTasks:
                            _config.OpenTheSettingsMenu = key;
                            break;
                        case ActionId.PotEdit:
                            _config.OpenTheFurnishingScreen = key;
                            break;
                        // TODO: 星之归还未找到
                        case ActionId.QuestList:
                            _config.OpenQuestMenu = key;
                            break;
                        case ActionId.NotificationDetails:
                            _config.OpenNotificationDetails = key;
                            break;
                        case ActionId.Chat:
                            _config.OpenChatScreen = key;
                            break;
                        case ActionId.EnvironmentInfo:
                            _config.OpenSpecialEnvironmentInformation = key;
                            break;
                        case ActionId.Tutorial:
                            _config.CheckTutorialDetails = key;
                            break;
                        case ActionId.ElementalSight:
                            _config.ElementalSight = key;
                            break;
                        case ActionId.ShowCursor:
                            _config.ShowCursor = key;
                            break;
                        case ActionId.PartySetup:
                            _config.OpenPartySetupScreen = key;
                            break;
                        case ActionId.Friends:
                            _config.OpenFriendsScreen = key;
                            break;
                         // TODO: 隐藏主界面未找到
                        default:
                            break;
                    }
                }
            }

            // 重新加载按键绑定列表
            KeyBindingSettingModels.Clear();
            BuildKeyBindingsList();
            var list = GetAllNonDirectoryKeyBinding(KeyBindingSettingModels);
            foreach (var keyConfig in list)
            {
                keyConfig.PropertyChanged += (sender, e) =>
                {
                    if (sender is KeyBindingSettingModel model)
                    {
                        // 使用反射更新配置文件
                        if (e.PropertyName == nameof(model.KeyValue))
                        {
                            Debug.WriteLine(_localizationService.GetString("keyBindings.bindingChanged", model.ActionName, model.KeyValue));

                            var pi = _config.GetType().GetProperty(model.ConfigPropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (pi != null && pi.CanWrite)
                            {
                                pi.SetValue(_config, model.KeyValue, null);
                            }
                        }
                    }
                };
            }
        }
    }
    
    
     /// <summary>
    /// 根据默认键位，将脚本中写死的按键映射为实际的按键
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static VK MappingKey(VK source)
    {
        if (TaskContext.Instance().Config.KeyBindingsConfig.GlobalKeyMappingEnabled)
        {
            // NOTE: 普攻、鼠标的冲刺、元素视野、视角居中和派蒙菜单不支持改键，此处不会进行映射
            return source switch
            {
                VK.VK_W => GIActions.MoveForward.ToActionKey().ToVK(),
                VK.VK_S => GIActions.MoveBackward.ToActionKey().ToVK(),
                VK.VK_A => GIActions.MoveLeft.ToActionKey().ToVK(),
                VK.VK_D => GIActions.MoveRight.ToActionKey().ToVK(),
                VK.VK_LCONTROL => GIActions.SwitchToWalkOrRun.ToActionKey().ToVK(),
                VK.VK_E => GIActions.ElementalSkill.ToActionKey().ToVK(),
                VK.VK_Q => GIActions.ElementalBurst.ToActionKey().ToVK(),
                VK.VK_LSHIFT => GIActions.SprintKeyboard.ToActionKey().ToVK(),
                VK.VK_R => GIActions.SwitchAimingMode.ToActionKey().ToVK(),
                VK.VK_SPACE => GIActions.Jump.ToActionKey().ToVK(),
                VK.VK_X => GIActions.Drop.ToActionKey().ToVK(),
                VK.VK_F => GIActions.PickUpOrInteract.ToActionKey().ToVK(),
                VK.VK_Z => GIActions.QuickUseGadget.ToActionKey().ToVK(),
                VK.VK_T => GIActions.InteractionInSomeMode.ToActionKey().ToVK(),
                VK.VK_V => GIActions.QuestNavigation.ToActionKey().ToVK(),
                VK.VK_P => GIActions.AbandonChallenge.ToActionKey().ToVK(),
                VK.VK_1 => GIActions.SwitchMember1.ToActionKey().ToVK(),
                VK.VK_2 => GIActions.SwitchMember2.ToActionKey().ToVK(),
                VK.VK_3 => GIActions.SwitchMember3.ToActionKey().ToVK(),
                VK.VK_4 => GIActions.SwitchMember4.ToActionKey().ToVK(),
                VK.VK_5 => GIActions.SwitchMember5.ToActionKey().ToVK(),
                VK.VK_TAB => GIActions.ShortcutWheel.ToActionKey().ToVK(),
                VK.VK_B => GIActions.OpenInventory.ToActionKey().ToVK(),
                VK.VK_C => GIActions.OpenCharacterScreen.ToActionKey().ToVK(),
                VK.VK_M => GIActions.OpenMap.ToActionKey().ToVK(),
                VK.VK_F1 => GIActions.OpenAdventurerHandbook.ToActionKey().ToVK(),
                VK.VK_F2 => GIActions.OpenCoOpScreen.ToActionKey().ToVK(),
                VK.VK_F3 => GIActions.OpenWishScreen.ToActionKey().ToVK(),
                VK.VK_F4 => GIActions.OpenBattlePassScreen.ToActionKey().ToVK(),
                VK.VK_F5 => GIActions.OpenTheEventsMenu.ToActionKey().ToVK(),
                VK.VK_F6 => GIActions.OpenTheSettingsMenu.ToActionKey().ToVK(),
                VK.VK_F7 => GIActions.OpenTheFurnishingScreen.ToActionKey().ToVK(),
                VK.VK_F8 => GIActions.OpenStellarReunion.ToActionKey().ToVK(),
                VK.VK_J => GIActions.OpenQuestMenu.ToActionKey().ToVK(),
                VK.VK_Y => GIActions.OpenNotificationDetails.ToActionKey().ToVK(),
                VK.VK_RETURN => GIActions.OpenChatScreen.ToActionKey().ToVK(),
                VK.VK_U => GIActions.OpenSpecialEnvironmentInformation.ToActionKey().ToVK(),
                VK.VK_G => GIActions.CheckTutorialDetails.ToActionKey().ToVK(),
                VK.VK_LMENU => GIActions.ShowCursor.ToActionKey().ToVK(),
                VK.VK_L => GIActions.OpenPartySetupScreen.ToActionKey().ToVK(),
                VK.VK_O => GIActions.OpenFriendsScreen.ToActionKey().ToVK(),
                VK.VK_OEM_2 => GIActions.HideUI.ToActionKey().ToVK(),
                // 其他按键（保留的？）不作转换
                _ => source,
            };
        }
        return source;
    }


}
