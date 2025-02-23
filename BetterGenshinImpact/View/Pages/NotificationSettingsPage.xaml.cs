using System.Windows.Controls;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.ViewModel.Pages;
using Wpf.Ui.Controls;

namespace BetterGenshinImpact.View.Pages
{
    public partial class NotificationSettingsPage : INavigableView<NotificationSettingsPageViewModel>
    {
        public NotificationSettingsPageViewModel ViewModel { get; }

        public NotificationSettingsPage(NotificationSettingsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            
            InitializeComponent();
        }

        // 可选：添加页面生命周期处理
        private void NotificationSettingsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 页面加载完成后的处理
            if (ViewModel != null)
            {
                // 可以添加一些初始化逻辑
            }
        }

        private void NotificationSettingsPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 页面卸载时的清理工作
        }
    }
}
