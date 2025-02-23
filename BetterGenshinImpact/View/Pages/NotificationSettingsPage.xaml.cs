using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BetterGenshinImpact.ViewModel.Pages;

namespace BetterGenshinImpact.View.Pages
{
    public partial class NotificationSettingsPage : Page
    {
        private NotificationSettingsPageViewModel ViewModel { get; }

        public NotificationSettingsPage(NotificationSettingsPageViewModel viewModel)
        {
            // 设置DataContext和保存ViewModel引用
            DataContext = ViewModel = viewModel;
            
            // 初始化组件
            InitializeComponent();

            // 注册页面加载事件
            Loaded += NotificationSettingsPage_Loaded;

            // 注册ViewModel属性变更事件
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void NotificationSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载完成后的初始化逻辑
            if (ViewModel != null)
            {
                // 可以在这里添加一些UI初始化逻辑
                // 例如: 根据配置状态更新UI显示
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 处理ViewModel属性变更事件
            switch (e.PropertyName)
            {
                case nameof(NotificationSettingsPageViewModel.IsLoading):
                    // 可以在这里处理加载状态变化
                    break;
                    
                case nameof(NotificationSettingsPageViewModel.WebhookStatus):
                case nameof(NotificationSettingsPageViewModel.EmailStatus):
                    // 可以在这里处理状态消息变化
                    break;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // 页面导航到此时的处理逻辑
            // 例如: 刷新配置数据
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            // 页面离开时的清理工作
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            
            Loaded -= NotificationSettingsPage_Loaded;
        }
    }
}
