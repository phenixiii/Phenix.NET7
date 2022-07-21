using System;
using System.Collections.Generic;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区贝位堆存
    /// </summary>
    [Serializable]
    public class AreaBayStack
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected internal AreaBayStack()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected AreaBayStack(IDictionary<int, IList<ContainerProperty>> tieredContainers)
        {
            _tieredContainers = tieredContainers;
        }

        #region 属性

        private IDictionary<int, IList<ContainerProperty>> _tieredContainers;

        /// <summary>
        /// 排号-叠箱
        /// </summary>
        public IDictionary<int, IList<ContainerProperty>> TieredContainers
        {
            get { return _tieredContainers ??= new Dictionary<int, IList<ContainerProperty>>(); }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="tieredContainers">排号-叠箱</param>
        public void OnRefresh(IDictionary<int, IList<ContainerProperty>> tieredContainers)
        {
            _tieredContainers = tieredContainers;
        }

        #endregion

        #endregion
    }
}