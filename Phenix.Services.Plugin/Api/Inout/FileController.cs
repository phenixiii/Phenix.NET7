﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.IO;
using Phenix.Services.Plugin.Actor.Inout;

namespace Phenix.Services.Plugin.Api.Inout
{
    /// <summary>
    /// 文件存取控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiInoutFilePath)]
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
            return await ClusterClient.Default.GetGrain<IFileGrain>(String.Empty).DownloadFileChunk(message, fileName, chunkNumber);
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
            return await ClusterClient.Default.GetGrain<IFileGrain>(String.Empty).UploadFileChunk(message,
                new FileChunkInfo(fileName, chunkCount, chunkNumber, chunkSize, maxChunkSize, await Request.ReadBodyAsFileChunkAsync()));
        }

        #endregion
    }
}
