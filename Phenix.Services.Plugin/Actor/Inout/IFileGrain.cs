using System.Threading.Tasks;
using Orleans;
using Phenix.Core.IO;

namespace Phenix.Services.Plugin.Actor.Inout
{
    /// <summary>
    /// 文件Grain接口
    /// </summary>
    public interface IFileGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 处理下载文件块
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="fileName">下载文件名</param>
        /// <param name="chunkNumber">块号</param>
        /// <returns>文件块信息</returns>
        Task<FileChunkInfo> DownloadFileChunk(string message, string fileName, int chunkNumber);

        /// <summary>
        /// 处理上传文件块
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="chunkInfo">文件块信息</param>
        /// <returns>完成上传时返回消息</returns>
        Task<string> UploadFileChunk(string message, FileChunkInfo chunkInfo);
    }
}
