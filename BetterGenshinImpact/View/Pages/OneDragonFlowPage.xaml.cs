using BetterGenshinImpact.ViewModel.Pages;
using BetterGenshinImpact.ViewModel.Pages;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using System.Linq; 
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BetterGenshinImpact.Service.Interface;

namespace BetterGenshinImpact.View.Pages;

public partial class OneDragonFlowPage
{
    public OneDragonFlowViewModel ViewModel { get; }
    
    private readonly Dictionary<CheckBox, string> _checkBoxMappings;
    private readonly ILocalizationService _localizationService;
    
    public static List<string> SereniteaPotSchedule =>
    [
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.everyday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.monday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.tuesday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.wednesday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.thursday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.friday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.saturday"),
        App.GetService<ILocalizationService>().GetString("oneDragon.weekdays.sunday")
    ];

    public OneDragonFlowPage(OneDragonFlowViewModel viewModel)
    {
        DataContext = ViewModel = viewModel;
        InitializeComponent();
        _localizationService = App.GetService<ILocalizationService>();
        _checkBoxMappings = new Dictionary<CheckBox, string>
        {
            { ClothCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.fabric") },
            { MomentResinCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.transientResin") },
            { SereniteapotExpBookCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.heroWit") },
            { SereniteapotExpBookSmallCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.wanderersAdvice") },
            { MagicmineralprecisionCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.mysticEnhancementOre") },
            { MOlaCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.mora") },
            { ExpBottleBigCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.sanctifyingEssence") },
            { ExpBottleSmallCheckBox, _localizationService.GetString("oneDragon.sereniteaPotItem.sanctifyingUnction") }
        };
        
    }
    
    private async void SereniteaPotTpType_Clicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedConfig == null)
        {
            return;
        }
        
        if (sender.Equals(PopupCloseButton)) Popup.IsOpen = false; //关闭弹窗
        
        else if (sender.Equals(PopupConfirmButton)) //确认选择
        {
            var selectedObjects = new List<string>(SereniteaPotComboBox.SelectedItem.ToString().Split('/'))
                .Concat(_checkBoxMappings.Where(pair => pair.Key.IsChecked == true).Select(pair => pair.Value)).ToList();

            ViewModel.SelectedConfig.SecretTreasureObjects = selectedObjects;
            Popup.IsOpen = false;
        }
        
        else if (sender.Equals(PotButton)) //初始化显示购买信息
        {
            if (!ViewModel.SelectedConfig.SecretTreasureObjects.Any())
            {
                ViewModel.SelectedConfig.SecretTreasureObjects.Add(_localizationService.GetString("oneDragon.weekdays.everyday"));
            }
          
            SereniteaPotComboBox.SelectedItem = ViewModel.SelectedConfig.SecretTreasureObjects[0];
            
            foreach (var pair in _checkBoxMappings)
            {
                pair.Key.IsChecked = ViewModel.SelectedConfig.SecretTreasureObjects.Contains(pair.Value);
            }

        }
        
    }
    
}
