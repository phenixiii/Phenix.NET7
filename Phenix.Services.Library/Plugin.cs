using Phenix.Core.Plugin;
using Phenix.Core.Security;
using Phenix.Services.Library.Security;

namespace Phenix.Services.Library
{
    /// <summary>
    /// 插件
    /// </summary>
    public class Plugin : PluginBase
    {
        #region 方法

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns>确定启动</returns>
        protected override bool Start()
        {
            Principal.FetchIdentity = Identity.Fetch;
            return base.Start();
        }
        
        #endregion
    }
}
