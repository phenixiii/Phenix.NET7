using System;
using System.Collections.Generic;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区
    /// </summary>
    [Serializable]
    public class YardArea
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected YardArea()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected YardArea(YardAreaProperty basis, IDictionary<int, IDictionary<int, YardBayRow>> bayRows)
            : this(basis)
        {
            _bayRows = bayRows;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="basis">基本要素</param>
        public YardArea(YardAreaProperty basis)
        {
            _basis = basis;
        }

        #region 属性

        private readonly YardAreaProperty _basis;

        /// <summary>
        /// 基本要素
        /// </summary>
        public YardAreaProperty Basis
        {
            get { return _basis; }
        }

        private readonly IDictionary<int, IDictionary<int, YardBayRow>> _bayRows = new Dictionary<int, IDictionary<int, YardBayRow>>();
        
        /// <summary>
        /// 贝位-排号-箱区贝排
        /// </summary>
        public IDictionary<int, IDictionary<int, YardBayRow>> BayRows
        {
            get { return  _bayRows; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取箱区贝排
        /// </summary>
        /// <param name="bayNo">贝位</param>
        /// <param name="rowNo">排号</param>
        /// <returns>箱区贝排</returns>
        public YardBayRow GetBayRow(int bayNo, int rowNo)
        {
            return _bayRows.GetValue(bayNo, () => new Dictionary<int, YardBayRow>(_basis.RowNumber)).GetValue(rowNo, () => new YardBayRow(this));
        }

        #region Event

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaCode">箱区代码</param>
        /// <param name="carryCycle">载运周期</param>
        public void OnVehicleOperation(string areaCode, int carryCycle)
        {
        }

        #endregion

        #endregion
    }
}