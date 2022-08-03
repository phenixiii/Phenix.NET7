using System;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Event;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// Grain基类
    /// </summary>
    public abstract class GrainBase : Phenix.Actor.GrainBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected GrainBase(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        #region 属性

        private readonly IEventBus _eventBus;

        /// <summary>
        /// 事件总线
        /// </summary>
        protected IEventBus EventBus => _eventBus;

        #region 配置项

        private static int? _timerIntervalSeconds; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// 定时器间隔(秒)
        /// 默认：1
        /// </summary>
        public static int TimerIntervalSeconds
        {
            get { return AppSettings.GetProperty(ref _timerIntervalSeconds, 1); }
            set { AppSettings.SetProperty(ref _timerIntervalSeconds, value); }
        }

        #endregion
        
        private IDisposable _timer;

        #endregion

        #region 方法

        /// <summary>
        /// 激活中
        /// </summary>
        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            _timer = RegisterTimer(ExecuteTimerAsync, null, TimeSpan.FromSeconds(TimerIntervalSeconds), TimeSpan.FromSeconds(TimerIntervalSeconds));
        }

        /// <summary>
        /// 失活中
        /// </summary>
        public override Task OnDeactivateAsync()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            return base.OnDeactivateAsync();
        }

        /// <summary>
        /// 定时TimerIntervalSeconds秒钟执行一次
        /// </summary>
        protected abstract Task ExecuteTimerAsync(object state);

        #endregion
    }
}