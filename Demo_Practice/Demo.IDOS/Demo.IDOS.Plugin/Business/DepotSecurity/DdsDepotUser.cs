using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-07-03 20:53:33
   mapping to: ddn_depot_user
*/

namespace Demo.IDOS.Plugin.Business.DepotSecurity
{
    /// <summary>
    /// 仓库用户
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "仓库用户")]
    public class DdsDepotUser : EntityBase<DdsDepotUser>
    {
        private DdsDepotUser()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DdsDepotUser(string dataSourceKey, long id, 
            long dpId, long usId, bool disabled)
            : base(dataSourceKey, id)
        {
            _dpId = dpId;
            _usId = usId;
            _disabled = disabled;
        }

        protected override void InitializeSelf()
        {
            _disabled = true;
        }

        private long _dpId;
        /// <summary>
        /// 仓库
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "仓库")]
        public long DpId
        {
            get { return _dpId; }
            set { _dpId = value; }
        }

        private long _usId;
        /// <summary>
        /// 用户
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "用户")]
        public long UsId
        {
            get { return _usId; }
            set { _usId = value; }
        }

        private bool _disabled;
        /// <summary>
        /// 是否禁用
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "是否禁用")]
        public bool Disabled
        {
            get { return _disabled; }
            set { _disabled = value; }
        }

    }
}
