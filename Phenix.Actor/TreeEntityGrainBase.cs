using System;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

namespace Phenix.Actor
{
    /// <summary>
    /// 树实体Grain基类
    /// </summary>
    public abstract class TreeEntityGrainBase<TKernel> : EntityGrainBase<TKernel>, ITreeEntityGrain<TKernel>
        where TKernel : TreeEntityBase<TKernel>
    {
        #region 属性

        private TKernel _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override TKernel Kernel
        {
            get { return _kernel ?? (_kernel = TreeEntityBase<TKernel>.FetchTree(Database, p => p.Id == Id)); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        Task<int> IEntityGrain<TKernel>.PatchKernel(params NameValue[] propertyValues)
        {
            return Task.FromResult(Kernel != null
                ? Kernel.UpdateSelf(propertyValues)
                : this is IGrainWithIntegerKey
                    ? TreeEntityBase<TKernel>.NewRoot(Database, Id, propertyValues).InsertSelf()
                    : TreeEntityBase<TKernel>.NewRoot(Database, propertyValues).InsertSelf());
        }

        private TKernel GetNode(long id)
        {
            if (Kernel == null)
                throw new ArgumentException("需先有根节点", nameof(id));
            TKernel node = Kernel.FindInBranch(p => p.Id == id);
            if (node == null)
                throw new ArgumentException(String.Format("找不到ID为{0}的节点", id), nameof(id));
            return node;
        }

        Task<long> ITreeEntityGrain<TKernel>.AddChildNode(long parentId, params NameValue[] propertyValues)
        {
            long result = Database.Sequence.Value;
            GetNode(parentId).AddChild(() => TreeEntityBase<TKernel>.New(Database, result, propertyValues));
            return Task.FromResult(result);
        }

        Task<int> ITreeEntityGrain<TKernel>.ChangeParentNode(long id, long parentId)
        {
            return Task.FromResult(GetNode(id).ChangeParent(GetNode(parentId)));
        }

        Task<int> ITreeEntityGrain<TKernel>.UpdateNode(long id, params NameValue[] propertyValues)
        {
            return Task.FromResult(GetNode(id).UpdateSelf(propertyValues));
        }

        Task<int> ITreeEntityGrain<TKernel>.DeleteBranch(long id)
        {
            return Task.FromResult(GetNode(id).DeleteBranch());
        }

        #endregion
    }
}
