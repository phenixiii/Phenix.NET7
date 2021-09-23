using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Services.Business.Security;

namespace Phenix.Services.Contract.Security
{
    /// <summary>
    /// 岗位资料Grain接口
    /// key: ID
    /// </summary>
    public interface IPositionGrain : IEntityGrain<Position>, IGrainWithIntegerKey
    {
        /// <summary>
        /// 确定是否属于指定的一组角色
        /// </summary>
        /// <param name="roles">指定的一组角色</param>
        /// <returns>存在交集</returns>
        Task<bool> IsInRole(IList<string> roles);
    }
}