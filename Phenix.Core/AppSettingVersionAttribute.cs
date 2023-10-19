using System;

namespace Phenix.Core
{
  /// <summary>
  /// "应用系统配置版本"标签
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Enum)]
  public sealed class AppSettingVersionAttribute : Attribute
  {
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="versionNumber">版本号</param>
    public AppSettingVersionAttribute(string versionNumber)
      : base()
    {
      _versionNumber = versionNumber;
    }

    #region 属性

    private readonly string _versionNumber;

    /// <summary>
    /// 版本号
    /// </summary>
    public string VersionNumber
    {
      get { return _versionNumber; }
    }

    #endregion
  }
}
