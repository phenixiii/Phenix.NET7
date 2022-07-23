using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 船舶进口船图
    /// </summary>
    public class VesselImportBayPlan
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected internal VesselImportBayPlan()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected VesselImportBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info)
        {
            _info = info;
        }

        #region 属性

        private IDictionary<int, IDictionary<int, IList<Container>>> _info;

        /// <summary>
        /// 贝位-排号-叠箱
        /// </summary>
        public IDictionary<int, IDictionary<int, IList<Container>>> Info
        {
            get { return _info ??= new Dictionary<int, IDictionary<int, IList<Container>>>(); }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="info">贝位-排号-叠箱</param>
        public void OnRefresh(IDictionary<int, IDictionary<int, IList<Container>>> info)
        {
            _info = info;
        }

        #endregion

        #endregion
    }
}