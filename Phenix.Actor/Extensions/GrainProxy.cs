using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Reflection;

namespace Phenix.Actor
{
    /// <summary>
    /// ClusterClient扩展
    /// </summary>
    public static class GrainProxy
    {
        #region EntityGrain

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <returns>是否存在</returns>
        public static async Task<bool> ExistKernelAsync<TGrain>(this IClusterClient clusterClient, string primaryKey)
            where TGrain : IEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).ExistKernel();
        }

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <returns>是否存在</returns>
        public static async Task<bool> ExistKernelAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey)
            where TGrain : IEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).ExistKernel();
        }

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <returns>是否存在</returns>
        public static async Task<bool> ExistKernelAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension)
            where TGrain : IEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).ExistKernel();
        }

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <returns>是否存在</returns>
        public static async Task<bool> ExistKernelAsync<TGrain>(this IClusterClient clusterClient, long primaryKey)
            where TGrain : IEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).ExistKernel();
        }

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <returns>是否存在</returns>
        public static async Task<bool> ExistKernelAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension)
            where TGrain : IEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).ExistKernel();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <returns>根实体对象</returns>
        public static async Task<TKernel> FetchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, string primaryKey)
            where TGrain : IEntityGrain<TKernel>, IGrainWithStringKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).FetchKernel();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <returns>根实体对象</returns>
        public static async Task<TKernel> FetchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, Guid primaryKey)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).FetchKernel();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <returns>根实体对象</returns>
        public static async Task<TKernel> FetchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).FetchKernel();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <returns>根实体对象</returns>
        public static async Task<TKernel> FetchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, long primaryKey)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).FetchKernel();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <returns>根实体对象</returns>
        public static async Task<TKernel> FetchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, long primaryKey, string keyExtension)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).FetchKernel();
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="source">数据源</param>
        public static async Task PatchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, string primaryKey,
            TKernel source)
            where TGrain : IEntityGrain<TKernel>, IGrainWithStringKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="source">数据源</param>
        public static async Task PatchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, Guid primaryKey,
            TKernel source)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="source">数据源</param>
        public static async Task PatchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            TKernel source)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).PatchKernel(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <param name="source">数据源</param>
        public static async Task PatchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, long primaryKey,
            TKernel source)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="source">数据源</param>
        public static async Task PatchKernelAsync<TGrain, TKernel>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            TKernel source)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).PatchKernel(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            params NameValue[] propertyValues)
            where TGrain : IEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            params NameValue[] propertyValues)
            where TGrain : IEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            params NameValue[] propertyValues)
            where TGrain : IEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            params NameValue[] propertyValues)
            where TGrain : IEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            params NameValue[] propertyValues)
            where TGrain : IEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            IDictionary<string, object> propertyValues)
            where TGrain : IEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            IDictionary<string, object> propertyValues)
            where TGrain : IEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            IDictionary<string, object> propertyValues)
            where TGrain : IEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            IDictionary<string, object> propertyValues)
            where TGrain : IEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task PatchKernelAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            IDictionary<string, object> propertyValues)
            where TGrain : IEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).PatchKernel(propertyValues);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain, TKernel>(this IClusterClient clusterClient, string primaryKey,
            Expression<Func<TKernel, object>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithStringKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain, TKernel>(this IClusterClient clusterClient, Guid primaryKey,
            Expression<Func<TKernel, object>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain, TKernel>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            Expression<Func<TKernel, object>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain, TKernel>(this IClusterClient clusterClient, long primaryKey,
            Expression<Func<TKernel, object>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<object> GetKernelPropertyAsync<TGrain, TKernel>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            Expression<Func<TKernel, object>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TValue>(this IClusterClient clusterClient, string primaryKey,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty<TValue>(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TValue>(this IClusterClient clusterClient, Guid primaryKey,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty<TValue>(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TValue>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty<TValue>(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TValue>(this IClusterClient clusterClient, long primaryKey,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty<TValue>(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TValue>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            string propertyName)
            where TGrain : IEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty<TValue>(propertyName);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TKernel, TValue>(this IClusterClient clusterClient, string primaryKey,
            Expression<Func<TKernel, TValue>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithStringKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty<TValue>(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TKernel, TValue>(this IClusterClient clusterClient, Guid primaryKey,
            Expression<Func<TKernel, TValue>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty<TValue>(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TKernel, TValue>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            Expression<Func<TKernel, TValue>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithGuidCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty<TValue>(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键String</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TKernel, TValue>(this IClusterClient clusterClient, long primaryKey,
            Expression<Func<TKernel, TValue>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).GetKernelProperty<TValue>(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public static async Task<TValue> GetKernelPropertyAsync<TGrain, TKernel, TValue>(this IClusterClient clusterClient, long primaryKey, string keyExtension,
            Expression<Func<TKernel, TValue>> propertyLambda)
            where TGrain : IEntityGrain<TKernel>, IGrainWithIntegerCompoundKey
            where TKernel : EntityBase<TKernel>
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).GetKernelProperty<TValue>(Utilities.GetPropertyInfo(propertyLambda).Name);
        }

        #endregion

        #region TreeEntityGrain

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            long parentId, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            long parentId, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            long parentId, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            long parentId, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension, 
            long parentId, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            long parentId, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            long parentId, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            long parentId, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            long parentId, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public static async Task<long> AddChildNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension, 
            long parentId, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        public static async Task ChangeParentNodeAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            long id, long parentId)
            where TGrain : ITreeEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).ChangeParentNode(id, parentId);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        public static async Task ChangeParentNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            long id, long parentId)
            where TGrain : ITreeEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).ChangeParentNode(id, parentId);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        public static async Task ChangeParentNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            long id, long parentId)
            where TGrain : ITreeEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).ChangeParentNode(id, parentId);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        public static async Task ChangeParentNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            long id, long parentId)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).ChangeParentNode(id, parentId);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        public static async Task ChangeParentNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension, 
            long id, long parentId)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).ChangeParentNode(id, parentId);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            long id, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).UpdateNode(id, propertyValues);
        }
        
        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            long id, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            long id, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            long id, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension, 
            long id, params NameValue[] propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            long id, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            long id, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            long id, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).UpdateNode(id, propertyValues);
        }
        
        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            long id, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public static async Task UpdateNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension, 
            long id, IDictionary<string, object> propertyValues)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        public static async Task<int> DeleteNodeAsync<TGrain>(this IClusterClient clusterClient, string primaryKey,
            long id)
            where TGrain : ITreeEntityGrain, IGrainWithStringKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).DeleteBranch(id);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        public static async Task<int> DeleteNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey,
            long id)
            where TGrain : ITreeEntityGrain, IGrainWithGuidKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).DeleteBranch(id);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        public static async Task<int> DeleteNodeAsync<TGrain>(this IClusterClient clusterClient, Guid primaryKey, string keyExtension,
            long id)
            where TGrain : ITreeEntityGrain, IGrainWithGuidCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).DeleteBranch(id);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        public static async Task<int> DeleteNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey,
            long id)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey).DeleteBranch(id);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="keyExtension">扩展主键</param>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        public static async Task<int> DeleteNodeAsync<TGrain>(this IClusterClient clusterClient, long primaryKey, string keyExtension, 
            long id)
            where TGrain : ITreeEntityGrain, IGrainWithIntegerCompoundKey
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return await clusterClient.GetGrain<TGrain>(primaryKey, keyExtension).DeleteBranch(id);
        }

        #endregion
    }
}