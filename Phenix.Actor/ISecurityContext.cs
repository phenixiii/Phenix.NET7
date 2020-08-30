namespace Phenix.Actor
{
    /// <summary>
    /// 安全上下文接口
    /// 要被具体的Grain接口继承才能为继承自这个接口的Grain实现在调用链上传递身份信息(Phenix.Core.Security.Identity.CurrentIdentity对象)
    /// </summary>
    public interface ISecurityContext
    {
    }
}
