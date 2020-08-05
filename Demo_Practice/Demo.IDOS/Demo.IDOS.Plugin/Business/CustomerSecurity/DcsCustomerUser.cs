using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-07-04 09:37:43
   mapping to: dcm_customer_user
*/

namespace Demo.IDOS.Plugin.Business.CustomerSecurity
{
    /// <summary>
    /// 客户用户
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "客户用户")]
    public class DcsCustomerUser : EntityBase<DcsCustomerUser>
    {
        private DcsCustomerUser()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DcsCustomerUser(string dataSourceKey, long id, 
            long cmId, long usId, bool disabled)
            : base(dataSourceKey, id)
        {
            _cmId = cmId;
            _usId = usId;
            _disabled = disabled;
        }

        protected override void InitializeSelf()
        {
            _disabled = true;
        }

        private long _cmId;
        /// <summary>
        /// 客户
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "客户")]
        public long CmId
        {
            get { return _cmId; }
            set { _cmId = value; }
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
