using System.Data.Common;
using Phenix.Core.Data.Schema;

namespace Phenix.Business
{
    internal interface IRefinedBusiness
    {
        #region 方法
        
        void SaveDepth(DbTransaction transaction, bool checkTimestamp);

        void SaveDepth(DbTransaction transaction, ExecuteAction executeAction, bool checkTimestamp);

        #endregion
    }
}
