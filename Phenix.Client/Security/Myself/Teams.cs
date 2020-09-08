using System;
using System.Collections.Generic;
using System.Net.Http;
using Phenix.Core.Data.Model;
using Phenix.Core.Net.Api;
using Phenix.Core.Threading;

namespace Phenix.Client.Security.Myself
{
    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public class Teams : TreeDataBase<Teams>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private Teams()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private Teams(string dataSourceKey, long id, string name, long rootId, long parentId, IList<Teams> allChildren)
            : base(dataSourceKey, id, rootId, parentId, allChildren)
        {
            _name = name;
        }

        private Teams(HttpClient httpClient, string name)
        {
            _httpClient = httpClient;
            _name = name;
        }

        #region 工厂

        internal static Teams Fetch(HttpClient httpClient)
        {
            Teams result = AsyncHelper.RunSync(() => httpClient.CallAsync<Teams>(HttpMethod.Get, ApiConfig.ApiSecurityMyselfRootTeamsPath, false));
            if (result != null)
                result._httpClient = httpClient;
            return result;
        }

        #endregion

        #region 属性

        [NonSerialized]
        private HttpClient _httpClient;

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加子团体
        /// </summary>
        /// <param name="name">名称</param>
        public Teams AddChild(string name)
        {
            return AddChild(() => new Teams(_httpClient, name),
                node => AsyncHelper.RunSync(() => _httpClient.CallAsync<long>(HttpMethod.Post, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                    Set(p => p.Name, node.Name).
                        Set(p => p.ParentId, node.ParentId))));
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="parentNode">父节点</param>
        public void ChangeParent(Teams parentNode)
        {
            ChangeParent(parentNode, () =>
                AsyncHelper.RunSync(() => _httpClient.CallAsync(HttpMethod.Put, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                    Set(p => p.Id, Id).Set(p => p.ParentId, parentNode.Id))));
        }

        /// <summary>
        /// 更新自己
        /// </summary>
        public void UpdateSelf()
        {
            AsyncHelper.RunSync(() => _httpClient.CallAsync(HttpMethod.Patch, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                Set(p => p.Id, Id).
                    Set(p => p.Name, Name)));
        }

        /// <summary>
        /// 删除本枝杈
        /// </summary>
        /// <returns>更新记录数</returns>
        public int DeleteBranch()
        {
            return DeleteBranch(() => AsyncHelper.RunSync(() => _httpClient.CallAsync<int>(HttpMethod.Delete, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                Set(p => p.Id, Id))));
        }

        #endregion
    }
}