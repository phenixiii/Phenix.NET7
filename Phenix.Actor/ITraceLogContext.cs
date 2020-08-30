namespace Phenix.Actor
{
    /// <summary>
    /// 追溯日志上下文接口
    /// 要被具体的Grain接口继承才能为继承自这个接口的Grain实现调用链的追溯日志记录(存放在主库PH7_EventLog表)
    /// </summary>
    public interface ITraceLogContext
    {
    }
}
