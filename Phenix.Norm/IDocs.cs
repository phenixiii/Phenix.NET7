using System;
using System.Linq;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;

namespace Phenix.Norm
{
    /// <summary>
    /// 资料接口
    /// </summary>
    public interface IDocs<T> : IEntity<T>
        where T : IDocs<T>
    {
        #region 属性

        /// <summary>
        /// 资料状态
        /// 仅允许本接口提供的VerifyDocs、ArchiveDocs方法间接更新并持久化（表字段建议命名为DOCS_STATUS_KEY）
        /// </summary>
        public string DocsStatusKey { get; }

        /// <summary>
        /// 资料状态
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DocsStatus DocsStatus
        {
            get
            {
                EnumKeyValue enumKeyValue = EnumKeyValue.Fetch<DocsStatus>().FirstOrDefault(p => p.Key == DocsStatusKey);
                return enumKeyValue != null ? (DocsStatus) enumKeyValue.Value : DocsStatus.Unverified;
            }
        }
        
        #endregion

        #region 方法

        /// <summary>
        /// 核对资料
        /// </summary>
        /// <param name="valid">是否有效</param>
        public void VerifyDocs(bool valid)
        {
            if (DocsStatus != DocsStatus.Unverified)
                throw new InvalidOperationException("VerifyDocs方法仅适用于未核对的资料!");

            UpdateSelf(NameValue.Set<T>(p => p.DocsStatusKey,
                valid ? EnumKeyValue.Fetch(DocsStatus.Valid).Key : EnumKeyValue.Fetch(DocsStatus.Discarded).Key));
        }

        /// <summary>
        /// 归档资料
        /// </summary>
        public void ArchiveDocs()
        {
            if (DocsStatus != DocsStatus.Valid)
                throw new InvalidOperationException("ArchiveDocs方法仅适用于有效的资料!");

            UpdateSelf(NameValue.Set<T>(p => p.DocsStatusKey, EnumKeyValue.Fetch(DocsStatus.Archived).Key));
        }

        #endregion
    }
}