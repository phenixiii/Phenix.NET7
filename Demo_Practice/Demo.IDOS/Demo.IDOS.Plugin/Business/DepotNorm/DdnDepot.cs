using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-07-03 20:53:33
   mapping to: ddn_depot
*/

namespace Demo.IDOS.Plugin.Business.DepotNorm
{
    /// <summary>
    /// 仓库
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "仓库")]
    public class DdnDepot : EntityBase<DdnDepot>
    {
        private DdnDepot()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DdnDepot(string dataSourceKey, long id, 
            string name, short unitNumber, short blockNumber, short platformNumber, short maxTier, short maxRow, short maxBay, bool shut)
            : base(dataSourceKey, id)
        {
            _name = name;
            _unitNumber = unitNumber;
            _blockNumber = blockNumber;
            _platformNumber = platformNumber;
            _maxTier = maxTier;
            _maxRow = maxRow;
            _maxBay = maxBay;
            _shut = shut;
        }

        protected override void InitializeSelf()
        {
            _unitNumber = 4;
            _blockNumber = 4;
            _platformNumber = 2;
            _maxTier = 8;
            _maxRow = 10;
            _maxBay = 20;
            _shut = false;
        }

        private string _name;
        /// <summary>
        /// 名称
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "名称")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private short _unitNumber;
        /// <summary>
        /// 单元数
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "单元数")]
        public short UnitNumber
        {
            get { return _unitNumber; }
            set { _unitNumber = value; }
        }

        private short _blockNumber;
        /// <summary>
        /// 存储区数/单元
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "存储区数/单元")]
        public short BlockNumber
        {
            get { return _blockNumber; }
            set { _blockNumber = value; }
        }

        private short _platformNumber;
        /// <summary>
        /// 站台数/存储区
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "站台数/存储区")]
        public short PlatformNumber
        {
            get { return _platformNumber; }
            set { _platformNumber = value; }
        }

        private short _maxTier;
        /// <summary>
        /// 最大层高
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "最大层高")]
        public short MaxTier
        {
            get { return _maxTier; }
            set { _maxTier = value; }
        }

        private short _maxRow;
        /// <summary>
        /// 最大排数
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "最大排数")]
        public short MaxRow
        {
            get { return _maxRow; }
            set { _maxRow = value; }
        }

        private short _maxBay;
        /// <summary>
        /// 最大贝位
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "最大贝位")]
        public short MaxBay
        {
            get { return _maxBay; }
            set { _maxBay = value; }
        }

        private bool _shut;
        /// <summary>
        /// 是否关闭
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "是否关闭")]
        public bool Shut
        {
            get { return _shut; }
            set { _shut = value; }
        }

    }
}
