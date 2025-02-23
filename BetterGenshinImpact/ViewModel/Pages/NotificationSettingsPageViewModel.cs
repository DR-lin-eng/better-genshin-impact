using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.Service.Notification;
using BetterGenshinImpact.Service.Notifier;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BetterGenshinImpact.ViewModel.Pages
{
    public partial class NotificationSettingsPageViewModel : ObservableObject, IViewModel
    {
        public AllConfig Config { get; set; }
        private readonly NotificationService _notificationService;

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

        // 邮件发送配置属性
        [ObservableProperty]
        private string _fromEmail = string.Empty;

        [ObservableProperty]
        private string _fromName = string.Empty;

        [ObservableProperty]
        private string _toEmail = string.Empty;

        public NotificationSettingsPageViewModel(IConfigService configService, NotificationService notificationService)
        {
            Config = configService.Get();
            _notificationService = notificationService;

            // 初始化邮件配置
            LoadEmailConfig();
        }

        private void LoadEmailConfig()
        {
            // 从Config加载邮件配置
            SmtpServer = Config.Notification.Email.SmtpServer;
            SmtpPort = Config.Notification.Email.SmtpPort;
            SmtpUsername = Config.Notification.Email.SmtpUsername;
            SmtpPassword = Config.Notification.Email.SmtpPassword;
            FromEmail = Config.Notification.Email.FromEmail;
            FromName = Config.Notification.Email.FromName;
            ToEmail = Config.Notification.Email.ToEmail;
        }

        [RelayCommand]
        private async Task OnSaveEmailConfig()
        {
            IsLoading = true;
            
            try
            {
                // 更新配置
                Config.Notification.Email.SmtpServer = SmtpServer;
                Config.Notification.Email.SmtpPort = SmtpPort;
                Config.Notification.Email.SmtpUsername = SmtpUsername;
                Config.Notification.Email.SmtpPassword = SmtpPassword;
                Config.Notification.Email.FromEmail = FromEmail;
                Config.Notification.Email.FromName = FromName;
                Config.Notification.Email.ToEmail = ToEmail;

                // 保存配置
                await Config.SaveAsync();
                
                EmailStatus = "配置保存成功";
            }
            catch (System.Exception ex)
            {
                EmailStatus = $"保存配置失败: {ex.Message}";
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
            var res = await _notificationService.TestNotifierAsync<WebhookNotifier>();
            WebhookStatus = res.Message;
            IsLoading = false;
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
