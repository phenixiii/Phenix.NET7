using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Phenix.Core.Data.Model;
using Phenix.Core.Reflection;

namespace Phenix.Actor
{
    /// <summary>
    /// ClusterClient扩展
    /// </summary>
    public static class GrainProxy
    {
        #region IEntityGrain

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="entityGrain">实体Grain接口</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyValue<TKernel, TValue>(this IEntityGrain<TKernel> entityGrain, Expression<Func<TKernel, TValue>> propertyLambda)
            where TKernel : EntityBase<TKernel>
        {
            if (entityGrain == null)
                throw new ArgumentNullException(nameof(entityGrain));

            return Utilities.ChangeType<TValue>(await entityGrain.GetKernelPropertyValue(Utilities.GetPropertyInfo(propertyLambda).Name));
        }

        #endregion
    }
}