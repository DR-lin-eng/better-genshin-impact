using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BetterGenshinImpact.Service.Notification.Model;
using BetterGenshinImpact.Service.Notifier.Exception;
using BetterGenshinImpact.Service.Notifier.Interface;

namespace BetterGenshinImpact.Service.Notifier
{
    public class EmailNotifier : INotifier
    {
        // 发件人配置
        private readonly string _fromEmail;
        private readonly string _fromName;

        // 提升 SmtpClient 为类的成员变量
        private readonly SmtpClient _smtpClient;
        private readonly string _smtpPassword;
        private readonly int _smtpPort;

        // SMTP服务器配置
        private readonly string _smtpServer;
        private readonly string _smtpUsername;

        public EmailNotifier(
            string smtpServer,
            int smtpPort,
            string smtpUsername,
            string smtpPassword,
            string fromEmail,
            string fromName,
            string toEmail = "")
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
            _fromEmail = fromEmail;
            _fromName = fromName;
            ToEmail = toEmail;

            // 在构造函数中初始化 SmtpClient
            _smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };
        }

        // 收件人邮箱
        public string ToEmail { get; set; }
        public string Name { get; set; } = "Email";

        public async Task SendAsync(BaseNotificationData content)
        {
            if (string.IsNullOrEmpty(ToEmail))
            {
                throw new NotifierException("收件人邮箱地址为空");
            }

            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = FormatEmailSubject(content),
                    Body = FormatEmailBody(content),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(ToEmail);

                // 添加图片处理
                if (content.Screenshot != null)
                {
                    // 获取图片数据
                    byte[] imageBytes;
                    using (var ms = new MemoryStream())
                    {
                        // 保存图片到内存流中
                        content.Screenshot.Save(ms, ImageFormat.Jpeg);
                        imageBytes = ms.ToArray();
                    }

                    // 添加为附件
                    using (var attachmentStream = new MemoryStream(imageBytes))
                    {
                        var attachment = new Attachment(attachmentStream, "screenshot.jpg", "image/jpeg");
                        mailMessage.Attachments.Add(attachment);
                    }

                    // 创建HTML视图
                    var htmlView = AlternateView.CreateAlternateViewFromString(
                        FormatEmailBodyWithImage(content),
                        null,
                        "text/html");

                    // 添加内联图片
                    using (var inlineStream = new MemoryStream(imageBytes))
                    {
                        var imageResource = new LinkedResource(inlineStream, "image/jpeg");
                        imageResource.ContentId = "screenshot";
                        htmlView.LinkedResources.Add(imageResource);

                        // 添加HTML视图
                        mailMessage.AlternateViews.Add(htmlView);
                    }
                }

                // 发送邮件
                await _smtpClient.SendMailAsync(mailMessage);
            }
            catch (System.Exception ex)
            {
                var errorMessage = string.Format("发送邮件失败: {0}", ex.Message);
                throw new NotifierException(errorMessage);
            }
        }

        private string FormatEmailSubject(BaseNotificationData content)
        {
            // 可以根据实际需求自定义邮件主题格式
            return string.Format("通知 - {0}", content.GetType().Name);
        }

        private string FormatEmailBody(BaseNotificationData content)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<html><body>");

            // 添加通知标题
            builder.AppendLine("<h2>通知详情</h2>");

            // 添加通知内容
            foreach (var prop in content.GetType().GetProperties())
            {
                // 跳过Screenshot属性，它会单独处理
                if (prop.Name == "Screenshot")
                    continue;

                var value = prop.GetValue(content);
                if (value != null)
                {
                    builder.AppendFormat("<p><strong>{0}:</strong> {1}</p>", prop.Name, value);
                    builder.AppendLine();
                }
            }

            builder.AppendLine("</body></html>");
            return builder.ToString();
        }

        private string FormatEmailBodyWithImage(BaseNotificationData content)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<html><body>");

            // 添加通知标题
            builder.AppendLine("<h2>通知详情</h2>");

            // 添加通知内容
            foreach (var prop in content.GetType().GetProperties())
            {
                // 跳过Screenshot属性，它会单独处理
                if (prop.Name == "Screenshot")
                    continue;

                var value = prop.GetValue(content);
                if (value != null)
                {
                    builder.AppendFormat("<p><strong>{0}:</strong> {1}</p>", prop.Name, value);
                    builder.AppendLine();
                }
            }

            // 添加图片，使用cid引用
            builder.AppendLine("<div style='margin-top: 20px;'>");
            builder.AppendLine("<p><strong>截图:</strong></p>");
            builder.AppendLine("<img src='cid:screenshot' alt='截图' style='max-width: 100%;' />");
            builder.AppendLine("</div>");

            builder.AppendLine("</body></html>");
            return builder.ToString();
        }
    }
}