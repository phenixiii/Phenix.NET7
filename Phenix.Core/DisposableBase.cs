using System;
using Phenix.Core.Reflection;

namespace Phenix.Core
{
    /// <summary>
    /// 释放资源基类
    /// </summary>
    public abstract class DisposableBase<T> : DisposableBase
        where T : DisposableBase<T>
    {
        #region 单例

        private static readonly object _defaultLock = new object();
        private static T _default;

        /// <summary>
        /// 单例
        /// </summary>
        public static T Default
        {
            get
            {
                if (_default == null)
                    lock (_defaultLock)
                        if (_default == null)
                        {
                            _default = (T)InstanceInfo.Fetch<T>().Create();
                        }

                return _default;
            }
        }

        #endregion

        #region 方法

        #region 实现 BaseDisposable 抽象函数

        /// <summary>
        /// 释放托管资源
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (_default == this)
                lock (_defaultLock)
                    if (_default == this)
                    {
                        _default = null;
                    }
        }

        /// <summary>
        /// 释放非托管资源
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// 释放资源基类
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        /// <summary>
        /// 释放资源基类
        /// </summary>
        ~DisposableBase()
        {
            Dispose(false);
        }

        #region 属性

        private bool _disposing;

        /// <summary>
        /// 正在释放中
        /// </summary>
        public bool Disposing
        {
            get { return _disposing; }
        }

        private bool _resourcesDisposed;

        #endregion

        #region 方法

        #region IDisposable 成员

        /// <summary>
        /// 释放自己
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing">如果为 true, 则释放托管资源和非托管资源; 如果为 false, 则仅释放非托管资源</param>
        private void Dispose(bool disposing)
        {
            _disposing = true;

            if (!_resourcesDisposed)
            {
                if (disposing)
                    DisposeManagedResources();
                DisposeUnmanagedResources();
                _resourcesDisposed = true;
            }
        }

        /// <summary>
        /// 释放托管资源
        /// </summary>
        protected abstract void DisposeManagedResources();

        /// <summary>
        /// 释放非托管资源
        /// </summary>
        protected abstract void DisposeUnmanagedResources();

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Close()
        {
            Dispose(true);
        }

        /// <summary>
        /// 尝试释放对象
        /// </summary>
        /// <param name="value">对象</param>
        public static bool TryDispose(object value)
        {
            if (value is IDisposable disposable)
            {
                disposable.Dispose();
                return true;
            }

            return false;
        }

        #endregion
    }
}
