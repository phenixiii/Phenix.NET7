﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Services.Business.Message;
using Phenix.Services.Contract.Message;

namespace Phenix.Services.Plugin.Message
{
    /// <summary>
    /// 用户消息Grain
    /// key：CompanyName'\u0004'UserName
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
            get { return AppSettings.GetProperty(ref _clearMessageDeferMonths, 6); }
            set { AppSettings.SetProperty(ref _clearMessageDeferMonths, value >= 3 ? value : 3); }
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