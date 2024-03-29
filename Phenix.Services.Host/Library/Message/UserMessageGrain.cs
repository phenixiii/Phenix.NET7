﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
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
    [StorageProvider]
    public class UserMessageGrain : GrainBase<IDictionary<long, UserMessage>>, IUserMessageGrain, IRemindable
    {
        #region 属性

        /// <summary>
        /// 用户复合键
        /// </summary>
        protected string UserPrimaryKey
        {
            get { return this.GetPrimaryKeyString(); }
        }

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

        async Task IUserMessageGrain.Send(string sender, string content)
        {
            State[Database.Sequence.Value] = new UserMessage(sender, UserPrimaryKey, content, DateTime.Now);
            await WriteStateAsync();
        }

        Task<IDictionary<long, UserMessage>> IUserMessageGrain.Receive()
        {
            return Task.FromResult(State);
        }

        async Task IUserMessageGrain.AffirmReceived(long id, bool burn)
        {
            if (burn)
                State.Remove(id);
            else if (State.TryGetValue(id, out UserMessage value))
                value.ReceivedTime = DateTime.Now;
            await WriteStateAsync();
        }

        /// <summary>
        /// 激活中
        /// </summary>
        public override async Task OnActivateAsync()
        {
            string reminderName = UserPrimaryKey;
            IGrainReminder reminder = await GetReminder(reminderName);
            if (reminder != null)
            {
                if (State.Count == 0)
                    await UnregisterReminder(reminder);
            }
            else
                await RegisterOrUpdateReminder(reminderName, TimeSpan.FromDays(1), TimeSpan.FromDays(7));

            await base.OnActivateAsync();
        }

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            if (State.Count > 0)
            {
                List<long> ids = new List<long>();
                foreach (KeyValuePair<long, UserMessage> kvp in State)
                    if (kvp.Value.CreateTime < DateTime.Now.AddMonths(-ClearMessageDeferMonths))
                        ids.Add(kvp.Key);
                if (ids.Count > 0)
                {
                    foreach (long item in ids)
                        State.Remove(item);
                    await WriteStateAsync();
                }
            }
        }

        #endregion
    }
}