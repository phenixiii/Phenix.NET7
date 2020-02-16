using System.Threading.Tasks;

namespace Phenix.Actor
{
    /// <summary>
    /// 根实体Grain接口
    /// </summary>
    public interface IRootEntityGrain
    {
        #region 方法

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        Task<string> SelectRecord();

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="propertyValues">待更新"属性名-属性值"键值队列(仅提交第一个属性映射的表)</param>
        /// <returns>更新记录数</returns>
        Task<int> UpdateRecord(string propertyValues);

        #endregion
    }
}