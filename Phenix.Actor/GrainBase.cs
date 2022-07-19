using System;
using Orleans;
using Orleans.Streams;
using Phenix.Core.Data;
using Phenix.Core.Security;

namespace Phenix.Actor
{
    /// <summary>
    /// Grain基类
    /// </summary>
    public abstract class GrainBase : Grain, IGrain
    {
        #region 属性

        /// <summary>
        /// 用户身份
        /// </summary>
        public Principal User
        {
            get { return Principal.CurrentPrincipal; }
        }

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected virtual Database Database
        {
            get { return Database.Default; }
        }

        private string _primaryKeyString;

        /// <summary>
        /// 主键String
        /// </summary>
        protected virtual string PrimaryKeyString
        {
            get
            {
                if (_primaryKeyString == null)
                {
                    if (this is IGrainWithStringKey)
                        SplitPrimaryKeyString();
                    else
                        throw new NotImplementedException("仅适用于IGrainWithStringKey接口");
                }

                return _primaryKeyString;
            }
        }

        private Guid? _primaryKeyGuid;

        /// <summary>
        /// 主键Guid
        /// </summary>
        protected virtual Guid PrimaryKeyGuid
        {
            get
            {
                if (!_primaryKeyGuid.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithGuidKey _:
                            _primaryKeyGuid = this.GetPrimaryKey();
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _primaryKeyGuid = this.GetPrimaryKey(out _primaryKeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("仅适用于IGrainWithGuidKey/IGrainWithGuidCompoundKey接口");
                    }
                }

                return _primaryKeyGuid.Value;
            }
        }

        private long? _primaryKeyLong;

        /// <summary>
        /// 主键Long
        /// </summary>
        protected virtual long PrimaryKeyLong
        {
            get
            {
                if (!_primaryKeyLong.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithIntegerKey _:
                            _primaryKeyLong = this.GetPrimaryKeyLong();
                            break;
                        case IGrainWithIntegerCompoundKey _:
                            _primaryKeyLong = this.GetPrimaryKeyLong(out _primaryKeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("仅适用于IGrainWithIntegerKey/IGrainWithIntegerCompoundKey接口");
                    }
                }

                return _primaryKeyLong.Value;
            }
        }

        private string _primaryKeyExtension;

        /// <summary>
        /// 扩展主键
        /// </summary>
        protected virtual string PrimaryKeyExtension
        {
            get
            {
                if (_primaryKeyExtension == null)
                {
                    switch (this)
                    {
                        case IGrainWithStringKey _:
                            SplitPrimaryKeyString();
                            break;
                        case IGrainWithIntegerCompoundKey _:
                            _primaryKeyLong = this.GetPrimaryKeyLong(out _primaryKeyExtension);
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _primaryKeyGuid = this.GetPrimaryKey(out _primaryKeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("仅适用于IGrainWithIntegerCompoundKey/IGrainWithGuidCompoundKey接口");
                    }
                }

                return _primaryKeyExtension;
            }
        }

        /// <summary>
        /// 流提供者
        /// </summary>
        protected virtual IStreamProvider StreamProvider
        {
            get { return ClusterClient.Default.GetStreamProvider(ContextKeys.SimpleMessageStreamProviderName); }
        }

        #endregion

        #region 方法

        private void SplitPrimaryKeyString()
        {
            string primaryKeyString = this.GetPrimaryKeyString();
            string[] strings = Standards.SplitCompoundKey(primaryKeyString);
            if (strings.Length == 2)
            {
                _primaryKeyString = strings[0];
                _primaryKeyExtension = strings[1];
            }
            else
                _primaryKeyString = primaryKeyString;
        }

        #endregion
    }

    /// <summary>
    /// Grain基类
    /// </summary>
    public abstract class GrainBase<TGrainState> : Grain<TGrainState>, IGrain
    {
        #region 属性

        /// <summary>
        /// 用户身份
        /// </summary>
        public Principal User
        {
            get { return Principal.CurrentPrincipal; }
        }

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected virtual Database Database
        {
            get { return Database.Default; }
        }

        private string _primaryKeyString;

        /// <summary>
        /// 主键String
        /// </summary>
        protected virtual string PrimaryKeyString
        {
            get
            {
                if (_primaryKeyString == null)
                {
                    if (this is IGrainWithStringKey)
                        SplitPrimaryKeyString();
                    else
                        throw new NotImplementedException("仅适用于IGrainWithStringKey接口");
                }

                return _primaryKeyString;
            }
        }

        private Guid? _primaryKeyGuid;

        /// <summary>
        /// 主键Guid
        /// </summary>
        protected virtual Guid PrimaryKeyGuid
        {
            get
            {
                if (!_primaryKeyGuid.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithGuidKey _:
                            _primaryKeyGuid = this.GetPrimaryKey();
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _primaryKeyGuid = this.GetPrimaryKey(out _primaryKeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("仅适用于IGrainWithGuidKey/IGrainWithGuidCompoundKey接口");
                    }
                }

                return _primaryKeyGuid.Value;
            }
        }

        private long? _primaryKeyLong;

        /// <summary>
        /// 主键Long
        /// </summary>
        protected virtual long PrimaryKeyLong
        {
            get
            {
                if (!_primaryKeyLong.HasValue)
                {
                    switch (this)
                    {
                        case IGrainWithIntegerKey _:
                            _primaryKeyLong = this.GetPrimaryKeyLong();
                            break;
                        case IGrainWithIntegerCompoundKey _:
                            _primaryKeyLong = this.GetPrimaryKeyLong(out _primaryKeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("仅适用于IGrainWithIntegerKey/IGrainWithIntegerCompoundKey接口");
                    }
                }

                return _primaryKeyLong.Value;
            }
        }

        private string _primaryKeyExtension;

        /// <summary>
        /// 扩展主键
        /// </summary>
        protected virtual string PrimaryKeyExtension
        {
            get
            {
                if (_primaryKeyExtension == null)
                {
                    switch (this)
                    {
                        case IGrainWithStringKey _:
                            SplitPrimaryKeyString();
                            break;
                        case IGrainWithIntegerCompoundKey _:
                            _primaryKeyLong = this.GetPrimaryKeyLong(out _primaryKeyExtension);
                            break;
                        case IGrainWithGuidCompoundKey _:
                            _primaryKeyGuid = this.GetPrimaryKey(out _primaryKeyExtension);
                            break;
                        default:
                            throw new NotImplementedException("仅适用于IGrainWithIntegerCompoundKey/IGrainWithGuidCompoundKey接口");
                    }
                }

                return _primaryKeyExtension;
            }
        }

        /// <summary>
        /// 流提供者
        /// </summary>
        protected virtual IStreamProvider StreamProvider
        {
            get { return ClusterClient.Default.GetStreamProvider(ContextKeys.SimpleMessageStreamProviderName); }
        }

        #endregion

        #region 方法

        private void SplitPrimaryKeyString()
        {
            string primaryKeyString = this.GetPrimaryKeyString();
            string[] strings = Standards.SplitCompoundKey(primaryKeyString);
            if (strings.Length == 2)
            {
                _primaryKeyString = strings[0];
                _primaryKeyExtension = strings[1];
            }
            else
                _primaryKeyString = primaryKeyString;
        }

        #endregion
    }
}