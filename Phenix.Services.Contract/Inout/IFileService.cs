using System.Threading.Tasks;

namespace Phenix.Services.Contract.Inout
{
    /// <summary>
    /// 文件存取服务接口
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 当完成下载文件时触发
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="sourcePath">读取路径</param>
        Task AfterDownloadFile(string message, string sourcePath);

        /// <summary>
        /// 当完成上传文件时触发
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="targetPath">写入路径</param>
        /// <returns>完成上传时返回消息</returns>
        Task<string> AfterUploadFile(string message, string targetPath);
    }
}
