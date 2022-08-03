using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 船舶出口船图
    /// </summary>
    public class VesselExportBayPlan
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected internal VesselExportBayPlan(IDictionary<int, IDictionary<int, IList<ContainerInfo>>> info = null)
        {
            _info = info;
        }

        #region 属性

        private IDictionary<int, IDictionary<int, IList<ContainerInfo>>> _info;

        /// <summary>
        /// 贝位-排号-叠箱
        /// </summary>
        public IDictionary<int, IDictionary<int, IList<ContainerInfo>>> Info
        {
            get { return _info ??= new Dictionary<int, IDictionary<int, IList<ContainerInfo>>>(); }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="info">贝位-排号-叠箱</param>
        public void OnRefresh(IDictionary<int, IDictionary<int, IList<ContainerInfo>>> info)
        {
            _info = info;
        }

        #endregion

        #endregion
    }
}