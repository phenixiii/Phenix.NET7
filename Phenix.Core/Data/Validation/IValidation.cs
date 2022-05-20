namespace Phenix.Core.Data.Validation
{
    /// <summary>
    /// 验证接口
    /// </summary>
    public interface IValidation
    {
        /// <summary>
        /// 验证上下文
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        /// <param name="validationContext">上下文</param>
        /// <returns>包含失败的验证信息</returns>
        System.ComponentModel.DataAnnotations.ValidationResult Validate(ExecuteAction executeAction, System.ComponentModel.DataAnnotations.ValidationContext validationContext);
    }
}
