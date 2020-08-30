using System;
using Orleans;
using Phenix.Core.Data;

namespace Phenix.Actor
{
    /// <summary>
    /// Grain基类
    /// </summary>
    public abstract class GrainBase : Grain
    {
        #region 属性

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected virtual Database Database
        {
            get { return Database.Default; }
        }

        private long? _id;

        /// <summary>
        /// ID(默认映射表主键XX_ID字段或获取自复合主键Key以默认映射AB关联表外键XX_A_ID字段之一)
        /// </summary>
        protected virtual long? Id
        {
            get
            {
                if (!_id.HasValue)
                {
                    if (this is IGrainWithIntegerKey)
                        _id = this.GetPrimaryKeyLong();
                    else if (this is IGrainWithIntegerCompoundKey)
                        _id = this.GetPrimaryKeyLong(out _keyExtension);
                }

                return _id;
            }
        }

        private long? _idExtension;

        /// <summary>
        /// ID扩展(如不为空说明获取自复合主键KeyExtension以默认映射AB关联表外键XX_B_ID字段之一)
        /// </summary>
        protected virtual long? IdExtension
        {
            get
            {
                if (!_idExtension.HasValue)
                {
                    if (this is IGrainWithIntegerCompoundKey)
                        _idExtension = Int64.Parse(KeyExtension);
                }

                return _idExtension;
            }
        }

        private string _keyExtension;

        /// <summary>
        /// 复合主键
        /// </summary>
        protected virtual string KeyExtension
        {
            get
            {
                if (_keyExtension == null)
                {
                    if (this is IGrainWithIntegerCompoundKey)
                        _id = this.GetPrimaryKeyLong(out _keyExtension);
                }

                return _keyExtension;
            }
        }

        #endregion
    }
}
