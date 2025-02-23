using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.Service.Notification;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BetterGenshinImpact.ViewModel.Pages
{
    public partial class NotificationSettingsPageViewModel : ObservableObject, IViewModel
    {
        public AllConfig Config { get; set; }
        private readonly NotificationService _notificationService;
        private readonly IConfigService _configService;

        [ObservableProperty] 
        private bool _isLoading;

        [ObservableProperty] 
        private string _webhookStatus = string.Empty;

        [ObservableProperty]
        private string _emailStatus = string.Empty;

        // SMTP服务器配置属性
        [ObservableProperty]
        private string _smtpServer = string.Empty;

        [ObservableProperty]
        private int _smtpPort;

        [ObservableProperty]
        private string _smtpUsername = string.Empty;

        [ObservableProperty]
        private string _smtpPassword = string.Empty;

        // 邮件配置属性
        [ObservableProperty]
        private string _fromEmail = string.Empty;

        [ObservableProperty]
        private string _fromName = string.Empty;

        [ObservableProperty]
        private string _toEmail = string.Empty;

        public NotificationSettingsPageViewModel(
            IConfigService configService, 
            NotificationService notificationService)
        {
            _configService = configService;
            Config = configService.Get();
            _notificationService = notificationService;

            // 初始化配置
            LoadEmailConfig();
        }

        private void LoadEmailConfig()
        {
            // 从 NotificationConfig 加载配置
            SmtpServer = Config.NotificationConfig.SmtpServer;
            SmtpPort = Config.NotificationConfig.SmtpPort;
            SmtpUsername = Config.NotificationConfig.SmtpUsername;
            SmtpPassword = Config.NotificationConfig.SmtpPassword;
            FromEmail = Config.NotificationConfig.FromEmail;
            FromName = Config.NotificationConfig.FromName;
            ToEmail = Config.NotificationConfig.ToEmail;
        }

        [RelayCommand]
        private async Task OnSaveEmailConfig()
        {
            IsLoading = true;
            EmailStatus = string.Empty;

            try
            {
                // 更新 NotificationConfig
                Config.NotificationConfig.SmtpServer = SmtpServer;
                Config.NotificationConfig.SmtpPort = SmtpPort;
                Config.NotificationConfig.SmtpUsername = SmtpUsername;
                Config.NotificationConfig.SmtpPassword = SmtpPassword;
                Config.NotificationConfig.FromEmail = FromEmail;
                Config.NotificationConfig.FromName = FromName;
                Config.NotificationConfig.ToEmail = ToEmail;

                // 保存配置
                await _configService.SaveAsync();
                EmailStatus = "配置已保存";
            }
            catch (System.Exception ex)
            {
                EmailStatus = $"保存失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task OnTestWebhook()
        {
            IsLoading = true;
            WebhookStatus = string.Empty;

            try
            {
                var res = await _notificationService.TestNotifierAsync<WebhookNotifier>();
                WebhookStatus = res.Message;
            }
            catch (System.Exception ex)
            {
                WebhookStatus = $"测试失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task OnTestEmail()
        {
            IsLoading = true;
            EmailStatus = string.Empty;

            try
            {
                var res = await _notificationService.TestNotifierAsync<EmailNotifier>();
                EmailStatus = res.Message;
            }
            catch (System.Exception ex)
            {
                EmailStatus = $"测试失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
