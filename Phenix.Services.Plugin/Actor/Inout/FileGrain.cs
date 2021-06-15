using System;
using System.IO;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Core.IO;
using Phenix.Services.Contract.Inout;

namespace Phenix.Services.Plugin.Actor.Inout
{
    /// <summary>
    /// 文件Grain
    /// </summary>
    public class FileGrain : GrainBase, IFileGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="service">注入文件存取服务</param>
        public FileGrain(IFileService service)
        {
            _service = service;
        }

        #region 属性
        
        #region 配置项

        private static string _downloadPath;

        /// <summary>
        /// 下载目录
        /// 默认：Phenix.Core.AppRun.TempDirectory
        /// </summary>
        public static string DownloadPath
        {
            get { return AppSettings.GetProperty(ref _downloadPath, AppRun.TempDirectory); }
            set { AppSettings.SetProperty(ref _downloadPath, value); }
        }

        private static int _chunkSize;

        /// <summary>
        /// 块最大值
        /// 默认：64 * 1024
        /// </summary>
        public static int MaxChunkSize
        {
            get { return AppSettings.GetProperty(ref _chunkSize, 64 * 1024); }
            set { AppSettings.SetProperty(ref _chunkSize, value); }
        }

        private static string _uploadPath;

        /// <summary>
        /// 上传目录
        /// 默认：Phenix.Core.AppRun.TempDirectory
        /// </summary>
        public static string UploadPath
        {
            get { return AppSettings.GetProperty(ref _uploadPath, AppRun.TempDirectory); }
            set { AppSettings.SetProperty(ref _uploadPath, value); }
        }

        #endregion

        private readonly IFileService _service;

        #endregion

        #region 方法

        /// <summary>
        /// 处理下载文件块
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="fileName">下载文件名</param>
        /// <param name="chunkNumber">块号</param>
        /// <returns>文件块信息</returns>
        async Task<FileChunkInfo> IFileGrain.DownloadFileChunk(string message, string fileName, int chunkNumber)
        {
            if (chunkNumber <= 0)
                return null;

            string sourcePath = Path.Combine(DownloadPath, fileName);
            if (!File.Exists(sourcePath))
                throw new InvalidOperationException(String.Format(AppSettings.GetValue("不存在下载文件: {0}"), fileName));

            await using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                FileChunkInfo result = await FileChunkInfo.CreateAsync(sourceStream, fileName, chunkNumber, MaxChunkSize);
                if (result.Done && _service != null)
                    await _service.AfterDownloadFile(message, sourcePath);
                return result;
            }
        }

        /// <summary>
        /// 处理上传文件块
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="chunkInfo">文件块信息</param>
        /// <returns>完成上传时返回消息</returns>
        async Task<string> IFileGrain.UploadFileChunk(string message, FileChunkInfo chunkInfo)
        {
            string targetPath = Path.Combine(UploadPath, chunkInfo.FileName);
            if (chunkInfo.Stop)
            {
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
            }
            else
            {
                try
                {
                    await using (FileStream targetStream = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        await chunkInfo.WriteAsync(targetStream);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                    throw;
                }

                if (chunkInfo.Done && _service != null)
                    return await _service.AfterUploadFile(message, targetPath);
            }

            return null;
        }

        #endregion
    }
}