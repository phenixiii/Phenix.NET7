using System;
using Orleans;
using Phenix.Core.Data;

namespace Phenix.Actor
{
    /// <summary>
    /// Grain基类
    /// </summary>
    public abstract class GrainBase : Grain, IGrain
    {
        #region 属性

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected virtual Database Database
        {
            get { return Database.Default; }
        }

        private string _name;

        /// <summary>
        /// Name
        /// </summary>
        protected virtual string Name
        {
            get
            {
                if (_name == null)
                {
                    if (this is IGrainWithStringKey)
                        _name = this.GetPrimaryKeyString();
                    else
                        throw new NotImplementedException("属性Name仅能用于实现IGrainWithStringKey接口的对象");
                }

                return _name;
            }
        }

        private long? _id;

        /// <summary>
        /// ID(默认映射表主键XX_ID字段或获取自复合主键Key以默认映射AB关联表外键XX_A_ID字段之一)
        /// </summary>
        protected virtual long Id
        {
            get
            {
                if (!_id.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithIntegerKey _:
                            _id = this.GetPrimaryKeyLong();
                            break;
                        case IGrainWithIntegerCompoundKey _:
                            _id = this.GetPrimaryKeyLong(out _keyExtension);
                            break;
                        default:
                            throw new NotImplementedException("属性ID仅能用于实现IGrainWithIntegerKey/IGrainWithIntegerCompoundKey接口的对象");
                    }
                }

                return _id.Value;
            }
        }

        private long? _idExtension;

        /// <summary>
        /// ID扩展(如不为空说明获取自复合主键KeyExtension以默认映射AB关联表外键XX_B_ID字段之一)
        /// </summary>
        protected virtual long IdExtension
        {
            get
            {
                if (!_idExtension.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithIntegerCompoundKey _:
                        case IGrainWithGuidCompoundKey _:
                            _idExtension = Int64.Parse(KeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("属性IdExtension仅能用于实现IGrainWithIntegerCompoundKey/IGrainWithGuidCompoundKey接口的对象");
                    }
                }

                return _idExtension.Value;
            }
        }

        private Guid? _guid;

        /// <summary>
        /// GuidKey
        /// </summary>
        protected virtual Guid Guid
        {
            get
            {
                if (!_guid.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithGuidKey _:
                            _guid = this.GetPrimaryKey();
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _guid = this.GetPrimaryKey(out _keyExtension);
                            break;
                        default:
                            throw new NotImplementedException("属性Guid仅能用于实现IGrainWithGuidKey/IGrainWithGuidCompoundKey接口的对象");
                    }
                }

                return _guid.Value;
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
                    switch (this)
                    {
                        case IGrainWithIntegerCompoundKey _:
                            _id = this.GetPrimaryKeyLong(out _keyExtension);
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _guid = this.GetPrimaryKey(out _keyExtension);
                            break;
                        default:
                            throw new NotImplementedException("属性KeyExtension仅能用于实现IGrainWithIntegerCompoundKey/IGrainWithGuidCompoundKey接口的对象");
                    }
                }

                return _keyExtension;
            }
        }

        #endregion
    }
}