using System;
using System.Collections.Generic;
using System.Net.Http;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
using Phenix.Core.Net.Api;

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
        private Teams(long id, string name, long rootId, long parentId, IList<Teams> allChildren)
            : base(id, rootId, parentId, allChildren)
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
            Teams result = httpClient.CallAsync<Teams>(HttpMethod.Get, ApiConfig.ApiSecurityMyselfRootTeamsPath, false).Result;
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
                node => _httpClient.CallAsync<long>(HttpMethod.Put, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                    NameValue.Set<Teams>(p => p.Name, node.Name),
                    NameValue.Set<Teams>(p => p.ParentId, node.ParentId)).Result);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <returns>更新记录数</returns>
        public int ChangeParent(Teams parentNode)
        {
            return ChangeParent(parentNode,
                () => _httpClient.CallAsync<int>(HttpMethod.Patch, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                    NameValue.Set<Teams>(p => p.Id, Id),
                    NameValue.Set<Teams>(p => p.ParentId, parentNode.Id)).Result);
        }

        /// <summary>
        /// 更新自己
        /// </summary>
        /// <returns>更新记录数</returns>
        public int UpdateSelf()
        {
            return _httpClient.CallAsync<int>(HttpMethod.Post, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                NameValue.Set<Teams>(p => p.Id, Id),
                NameValue.Set<Teams>(p => p.Name, Name)).Result;
        }

        /// <summary>
        /// 删除本枝杈
        /// </summary>
        /// <returns>更新记录数</returns>
        public int DeleteBranch()
        {
            return DeleteBranch(() => _httpClient.CallAsync<int>(HttpMethod.Delete, ApiConfig.ApiSecurityMyselfRootTeamsNodePath,
                NameValue.Set<Teams>(p => p.Id, Id)).Result);
        }

        #endregion
    }
}