using System;
using System.Collections.Generic;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区贝排
    /// </summary>
    [Serializable]
    public class YardBayRow
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner">主人</param>
        public YardBayRow(YardArea owner)
        {
            _owner = owner;
        }

        #region 属性

        private readonly YardArea _owner;

        /// <summary>
        /// 主人
        /// </summary>
        public YardArea Owner
        {
            get { return _owner; }
        }

        private readonly IList<ContainerProperty> _tieredContainers = new List<ContainerProperty>();

        /// <summary>
        /// 层叠的箱
        /// </summary>
        public IList<ContainerProperty> TieredContainers
        {
            get { return _tieredContainers; }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="voyage">航次</param>
        /// <param name="bayPlan">贝-船图箱</param>
        public void Refresh(IList<ContainerProperty> tieredContainers)
        {
        }

        #endregion

        #endregion
    }
}