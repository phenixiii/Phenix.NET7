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

        private Guid? _guid;

        /// <summary>
        /// 主键
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
                            _guid = this.GetPrimaryKey(out _extensionName);
                            break;
                        default:
                            throw new NotImplementedException("属性Guid仅能用于实现IGrainWithGuidKey/IGrainWithGuidCompoundKey接口的对象");
                    }
                }

                return _guid.Value;
            }
        }

        private string _name;

        /// <summary>
        /// 主键
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
        /// 主键
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
                            _id = this.GetPrimaryKeyLong(out _extensionName);
                            break;
                        default:
                            throw new NotImplementedException("属性ID仅能用于实现IGrainWithIntegerKey/IGrainWithIntegerCompoundKey接口的对象");
                    }
                }

                return _id.Value;
            }
        }

        private string _extensionName;

        /// <summary>
        /// 扩展主键
        /// </summary>
        protected virtual string ExtensionName
        {
            get
            {
                if (_extensionName == null)
                {
                    switch (this)
                    {
                        case IGrainWithIntegerCompoundKey _:
                            _id = this.GetPrimaryKeyLong(out _extensionName);
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _guid = this.GetPrimaryKey(out _extensionName);
                            break;
                        default:
                            throw new NotImplementedException("属性ExtensionName仅能用于实现IGrainWithIntegerCompoundKey/IGrainWithGuidCompoundKey接口的对象");
                    }
                }

                return _extensionName;
            }
        }

        private long? _extensionId;

        /// <summary>
        /// 扩展主键
        /// </summary>
        protected virtual long ExtensionId
        {
            get
            {
                if (!_extensionId.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithIntegerCompoundKey _:
                        case IGrainWithGuidCompoundKey _:
                            _extensionId = Int64.Parse(ExtensionName);
                            break;
                        default:
                            throw new NotImplementedException("属性ExtensionId仅能用于实现IGrainWithIntegerCompoundKey/IGrainWithGuidCompoundKey接口的对象");
                    }
                }

                return _extensionId.Value;
            }
        }

        #endregion
    }
}