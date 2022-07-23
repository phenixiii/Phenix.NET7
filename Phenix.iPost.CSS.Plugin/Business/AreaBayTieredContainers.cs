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
        /// for CreateInstance
        /// </summary>
        protected internal AreaBayTieredContainers()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected AreaBayTieredContainers(IDictionary<int, IList<Container>> info)
        {
            _info = info;
        }

        #region 属性

        private IDictionary<int, IList<Container>> _info;

        /// <summary>
        /// 排号-叠箱
        /// </summary>
        public IDictionary<int, IList<Container>> Info
        {
            get { return _info ??= new Dictionary<int, IList<Container>>(); }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="info">排号-叠箱</param>
        public void OnRefresh(IDictionary<int, IList<Container>> info)
        {
            _info = info;
        }

        #endregion

        #endregion
    }
}