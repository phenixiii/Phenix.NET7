using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-07-03 20:53:33
   mapping to: dcm_customer
*/

namespace Demo.IDOS.Plugin.Business.CustomerManagement
{
    /// <summary>
    /// 客户
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "客户")]
    public class DcmCustomer : EntityBase<DcmCustomer>
    {
        private DcmCustomer()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DcmCustomer(string dataSourceKey, long id, 
            string code, string shortName, string name, bool disabled)
            : base(dataSourceKey, id)
        {
            _code = code;
            _shortName = shortName;
            _name = name;
            _disabled = disabled;
        }

        protected override void InitializeSelf()
        {
            _disabled = false;
        }

        private string _code;
        /// <summary>
        /// 客户编码
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "客户编码")]
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }

        private string _shortName;
        /// <summary>
        /// 客户简称
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "客户简称")]
        public string ShortName
        {
            get { return _shortName; }
            set { _shortName = value; }
        }

        private string _name;
        /// <summary>
        /// 客户名称
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "客户名称")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
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
