using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Actor
{
    /// <summary>
    /// 岗位资料Grain接口
    /// </summary>
    public interface IPositionGrain : IEntityGrain<Position>, IGrainWithIntegerKey
    {
        /// <summary>
        /// 确定是否属于指定的角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns>属于指定的角色</returns>
        Task<bool> IsInRole(string role);
    }
}