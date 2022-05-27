using System;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 船舶
    /// </summary>
    [Serializable]
    public class Vessel
    {
        /// <summary>
        /// 船舶
        /// </summary>
        /// <param name="vesselCode">船舶ID</param>
        /// <param name="vesselName">船舶名称</param>
        public Vessel(string vesselCode, string vesselName)
        {
            _vesselCode = vesselCode;
            _vesselName = vesselName;
        }

        #region 属性

        private readonly string _vesselCode;

        /// <summary>
        /// 船舶ID
        /// </summary>
        public string VesselCode
        {
            get { return _vesselCode; }
        }

        private readonly string _vesselName;

        /// <summary>
        /// 船舶名称
        /// </summary>
        public string VesselName
        {
            get { return _vesselName; }
        }

        private VesselStatus _vesselStatus;

        /// <summary>
        /// 船舶状态
        /// </summary>
        public VesselStatus VesselStatus
        {
            get { return _vesselStatus; }
        }

        private string _vesselInVoyage;

        /// <summary>
        /// 进口航次
        /// </summary>
        public string VesselInVoyage
        {
            get { return _vesselInVoyage; }
        }
        
        private string _vesselOutVoyage;

        /// <summary>
        /// 出口航次
        /// </summary>
        public string VesselOutVoyage
        {
            get { return _vesselOutVoyage; }
        }

        private VesselAlongSideProperty? _alongSide;

        /// <summary>
        /// 靠泊信息
        /// </summary>
        public VesselAlongSideProperty? AlongSide
        {
            get { return _alongSide; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 靠泊
        /// </summary>
        /// <param name="vesselInVoyage">进口航次</param>
        /// <param name="vesselOutVoyage">出口航次</param>
        /// <param name="alongSide">靠泊信息</param>
        public void Berth(string vesselInVoyage, string vesselOutVoyage, VesselAlongSideProperty alongSide)
        {
            _vesselStatus = VesselStatus.Berthed;
            _vesselInVoyage = vesselInVoyage;
            _vesselOutVoyage = vesselOutVoyage;
            _alongSide = alongSide;
        }

        /// <summary>
        /// 离港
        /// </summary>
        public void Depart()
        {
            _vesselStatus = VesselStatus.Departed;
        }

        #endregion
    }
}