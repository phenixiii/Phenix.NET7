using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Phenix.Actor;
using Phenix.Core;

namespace Phenix.Services.Host.Library.Message
{
    /// <summary>
    /// 用户消息Grain
    /// key: CompanyName
    /// keyExtension: UserName
    /// </summary>
    public class UserMessageGrain : GrainBase, IUserMessageGrain, IRemindable
    {
        #region 属性

        #region 配置项

        private static int? _clearMessageDeferMonths;

        /// <summary>
        /// 清理几个月前的消息
        /// 默认：6(>=3)
        /// </summary>
        public static int ClearMessageDeferMonths
        {
            get { return new[] { AppSettings.GetProperty(ref _clearMessageDeferMonths, 6), 3 }.Max(); }
            set { AppSettings.SetProperty(ref _clearMessageDeferMonths, new[] { value, 3 }.Max()); }
        }

        #endregion

        #endregion

        #region 方法

        Task IUserMessageGrain.Send(string receiver, string content)
        {
            Database.Execute(UserMessage.Send, Database.Sequence.Value, this.GetPrimaryKeyString(), receiver, content);
            return Task.CompletedTask;
        }

        Task<IDictionary<long, string>> IUserMessageGrain.Receive()
        {
            return Task.FromResult(Database.ExecuteGet(UserMessage.Receive, this.GetPrimaryKeyString()));
        }

        Task IUserMessageGrain.AffirmReceived(long id, bool burn)
        {
            Database.Execute(UserMessage.AffirmReceived, id, burn);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 激活中
        /// </summary>
        public override async Task OnActivateAsync()
        {
            await RegisterOrUpdateReminder(this.GetPrimaryKeyString(), TimeSpan.FromDays(1), TimeSpan.FromDays(7));
            await base.OnActivateAsync();
        }

        Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            Database.Execute(UserMessage.Clear, this.GetPrimaryKeyString(), ClearMessageDeferMonths);
            return Task.CompletedTask;
        }

        #endregion
    }
}