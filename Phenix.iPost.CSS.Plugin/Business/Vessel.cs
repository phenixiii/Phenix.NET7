using System.Collections.Generic;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 船舶
    /// </summary>
    public class Vessel
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="vesselCode">船舶代码</param>
        public Vessel(string vesselCode)
        {
            _vesselCode = vesselCode;
        }

        #region 属性

        private readonly string _vesselCode;

        /// <summary>
        /// 船舶代码
        /// </summary>
        public string VesselCode
        {
            get { return _vesselCode; }
        }

        private string _vesselName;

        /// <summary>
        /// 船舶名称
        /// </summary>
        public string VesselName
        {
            get { return _vesselName; }
            set { _vesselName = value; }
        }

        private IDictionary<int, ContainerProperty> _bayPlan;

        /// <summary>
        /// 贝位-船图箱
        /// </summary>
        public IDictionary<int, ContainerProperty> BayPlan
        {
            get { return  _bayPlan; }
        }

        private VesselStatus _vesselStatus;

        /// <summary>
        /// 船舶状态
        /// </summary>
        public VesselStatus VesselStatus
        {
            get { return _vesselStatus; }
        }

        private Property.VesselAlongSideProperty? _alongSide;

        /// <summary>
        /// 靠泊信息
        /// </summary>
        public Property.VesselAlongSideProperty? AlongSide
        {
            get { return _alongSide; }
        }

        #endregion

        #region 方法
        
        #region Event

        /// <summary>
        /// 设置进口船图
        /// </summary>
        /// <param name="voyage">航次</param>
        /// <param name="bayPlan">贝位-船图箱</param>
        public void SetBayPlan(string voyage, IDictionary<int, ContainerProperty> bayPlan)
        {
            _bayPlan = bayPlan;
        }

        /// <summary>
        /// 靠泊
        /// </summary>
        /// <param name="terminalCode">码头代码</param>
        /// <param name="voyage">航次</param>
        /// <param name="alongSide">靠泊信息</param>
        public void OnBerth(string terminalCode, string voyage, VesselAlongSideProperty alongSide)
        {
            _vesselStatus = VesselStatus.Berthed;
            _alongSide = alongSide;
        }

        /// <summary>
        /// 离泊
        /// </summary>
        /// <param name="terminalCode">码头代码</param>
        /// <param name="voyage">航次</param>
        public void OnDepart(string terminalCode, string voyage)
        {
            _vesselStatus = VesselStatus.Departed;
        }
        
        #endregion

        #endregion
    }
}