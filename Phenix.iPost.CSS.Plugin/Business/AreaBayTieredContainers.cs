using System;
using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区贝位堆箱
    /// </summary>
    [Serializable]
    public class AreaBayTieredContainers
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected internal AreaBayTieredContainers(IDictionary<int, IList<ContainerInfo>> info = null)
        {
            _info = info;
        }

        #region 属性

        private IDictionary<int, IList<ContainerInfo>> _info;

        /// <summary>
        /// 排号-叠箱
        /// </summary>
        public IDictionary<int, IList<ContainerInfo>> Info
        {
            get { return _info ??= new Dictionary<int, IList<ContainerInfo>>(); }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="info">排号-叠箱</param>
        public void OnRefresh(IDictionary<int, IList<ContainerInfo>> info)
        {
            _info = info;
        }

        #endregion

        #endregion
    }
}