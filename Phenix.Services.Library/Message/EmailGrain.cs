using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Phenix.Actor;
using Phenix.Core;

namespace Phenix.Services.Library.Message
{
    /// <summary>
    /// 邮箱Grain
    /// key: Name
    /// </summary>
    public class EmailGrain : GrainBase, IEmailGrain
    {
        #region 属性

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return PrimaryKeyString; }
        }

        #region 配置项

        private string _senderHost;

        /// <summary>
        /// 发送邮箱服务
        /// </summary>
        public string SenderHost
        {
            get { return AppSettings.GetProperty(Name, ref _senderHost, "smtp.163.com"); }
            set { AppSettings.SetProperty(Name, ref _senderHost, value); }
        }

        private int? _senderPort;

        /// <summary>
        /// 发送邮箱端口
        /// </summary>
        public int SenderPort
        {
            get { return AppSettings.GetProperty(Name, ref _senderPort, 465); }
            set { AppSettings.SetProperty(Name, ref _senderPort, value); }
        }

        private bool? _senderEnabledSsl;

        /// <summary>
        /// 发送用SSL
        /// </summary>
        public bool SenderEnabledSsl
        {
            get { return AppSettings.GetProperty(Name, ref _senderEnabledSsl, true); }
            set { AppSettings.SetProperty(Name, ref _senderEnabledSsl, value); }
        }

        private string _senderAddress;

        /// <summary>
        /// 发送地址
        /// </summary>
        public string SenderAddress
        {
            get { return AppSettings.GetProperty(Name, ref _senderAddress, "phenixiii@163.com"); }
            set { AppSettings.SetProperty(Name, ref _senderAddress, value); }
        }

        private string _senderPassword;

        /// <summary>
        /// 发送口令
        /// </summary>
        public string SenderPassword
        {
            get { return AppSettings.GetProperty(Name, ref _senderPassword, "password", true); }
            set { AppSettings.SetProperty(Name, ref _senderPassword, value, true); }
        }

        private int? _sendTimeoutMilliseconds;

        /// <summary>
        /// 发送超时毫秒
        /// </summary>
        public int SendTimeoutMilliseconds
        {
            get { return AppSettings.GetProperty(Name, ref _sendTimeoutMilliseconds, 10000); }
            set { AppSettings.SetProperty(Name, ref _sendTimeoutMilliseconds, value); }
        }

        #endregion

        #endregion

        #region 方法

        async Task IEmailGrain.Send(string receiver, string receiverAddress, string subject, bool urgent, string htmlBody)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(Name, SenderAddress));
            message.To.Add(new MailboxAddress(receiver, receiverAddress));
            message.Subject = subject;
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlBody;
            message.Body = bodyBuilder.ToMessageBody();
            message.Priority = urgent ? MessagePriority.Urgent : MessagePriority.Normal;
            using (SmtpClient client = new SmtpClient())
            {
                client.Timeout = SendTimeoutMilliseconds;
                client.ServerCertificateValidationCallback = delegate { return true; };
                CancellationTokenSource tokenSource = new CancellationTokenSource(SendTimeoutMilliseconds);
                await client.ConnectAsync(SenderHost, SenderPort, SenderEnabledSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto, tokenSource.Token);
                await client.AuthenticateAsync(SenderAddress, SenderPassword, tokenSource.Token);
                await client.SendAsync(message, tokenSource.Token);
                await client.DisconnectAsync(true, tokenSource.Token);
            }
        }

        #endregion
    }
}