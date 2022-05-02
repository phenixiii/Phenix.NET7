using System;
using System.Threading;
using System.Threading.Tasks;

namespace Phenix.Core.Threading
{
    /// <summary>
    /// 异步帮助
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// 在阻塞上下文中执行异步代码
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            _taskFactory.StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// 在阻塞上下文中执行异步代码
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="task">异步任务</param>
        /// <returns>返回值</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> task)
        {
            return _taskFactory.StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}
