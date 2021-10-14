using Orleans;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: Manager（PH7_User.US_ID）
    /// keyExtension: Standards.FormatYearMonth(year, month)
    /// </summary>
    public interface IWorkScheduleGrain : Phenix.Actor.IEntityGrain<WorkSchedule>, IGrainWithIntegerCompoundKey
    {
    }
}
