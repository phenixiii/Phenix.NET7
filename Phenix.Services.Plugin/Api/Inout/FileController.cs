using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.IO;

namespace Phenix.Services.Plugin.Api.Inout
{
    /// <summary>
    /// 文件存取控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiInoutFilePath)]
    [ApiController]
    public sealed class FileController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="service">注入文件存取服务</param>
        public FileController(IFileService service)
        {
            _service = service;
        }

        #region 属性

        private readonly IFileService _service;

        #endregion

        #region 方法

        // phAjax.downloadFileChunk
        /// <summary>
        /// 下载文件块
        /// </summary>
        /// <returns>文件块信息</returns>
        [Authorize]
        [HttpGet]
        public async Task<FileChunkInfo> DownloadFileChunk(string message, string fileName, int chunkNumber, CancellationToken cancellationToken)
        {
            return await Request.DownloadFileChunkAsync(message, fileName, chunkNumber, _service, cancellationToken);
        }

        // phAjax.uploadFileChunk
        /// <summary>
        /// 上传文件块
        /// </summary>
        /// <returns>完成上传时返回消息</returns>
        [Authorize]
        [HttpPut]
        public async Task<string> UploadFileChunk(CancellationToken cancellationToken)
        {
            return await Request.UploadFileChunkAsync(_service, cancellationToken);
        }

        #endregion
    }
}
