using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 吊车
    /// </summary>
    public abstract class Crane : Machine
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="machineId">机械ID</param>
        protected Crane(string machineId)
            : base(machineId)
        {
        }

        #region 属性

        private bool _inPlace;

        /// <summary>
        /// 已就位
        /// </summary>
        public bool InPlace
        {
            get { return _inPlace; }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 有动作
        /// </summary>
        /// <param name="action">动作</param>
        public virtual void OnAction(CraneAction action)
        {
            _inPlace = action == CraneAction.InPlace;
        }

        /// <summary>
        /// 有抓具动作
        /// </summary>
        /// <param name="grabAction">抓具动作</param>
        /// <param name="hoistHeight">起升高度cm</param>
        public virtual void OnAction(CraneGrabAction grabAction, int hoistHeight)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}