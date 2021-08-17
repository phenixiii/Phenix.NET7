using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Services.Business.Inout;
using Phenix.Services.Contract;
using Phenix.Services.Contract.Inout;

namespace Phenix.Services.Plugin.Inout
{
    /// <summary>
    /// 文件存取控制器
    /// </summary>
    [Route(WebApiConfig.ApiInoutFilePath)]
    [ApiController]
    public sealed class FileController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        // phAjax.downloadFileChunk
        /// <summary>
        /// 下载文件块
        /// </summary>
        /// <returns>文件块信息</returns>
        [Authorize]
        [HttpGet]
        public async Task<FileChunkInfo> DownloadFileChunk(string message, string fileName, int chunkNumber)
        {
            return await ClusterClient.Default.GetGrain<IFileGrain>(this.GetType().FullName).DownloadFileChunk(message, fileName, chunkNumber);
        }

        // phAjax.uploadFileChunk
        /// <summary>
        /// 上传文件块
        /// </summary>
        /// <returns>完成上传时返回消息</returns>
        [Authorize]
        [HttpPut]
        public async Task<string> UploadFileChunk(string message, string fileName, int chunkCount, int chunkNumber, int chunkSize, int maxChunkSize)
        {
            return await ClusterClient.Default.GetGrain<IFileGrain>(this.GetType().FullName).UploadFileChunk(message,
                new FileChunkInfo(fileName, chunkCount, chunkNumber, chunkSize, maxChunkSize, await Request.ReadBodyAsFileChunkAsync()));
        }

        #endregion
    }
}
