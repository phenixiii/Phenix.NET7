using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;

namespace Phenix.Services.Library.Security.Myself
{
    /// <summary>
    /// 公司Grain接口
    /// key: CompanyName
    /// </summary>
    public interface ICompanyGrain : ITreeEntityGrain<Teams>, IGrainWithStringKey
    {
        /// <summary>
        /// 确定是否有效（含Children内容）
        /// </summary>
        Task<bool> IsValid();
    }
}