using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Phenix.Core.DependencyInjection;
using Phenix.Services.Contract;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 文件存取服务
    /// </summary>
    [Service(typeof(IFileService), ServiceLifetime.Transient)]
    public sealed class FileService : IFileService
    {
        #region 方法

        Task IFileService.AfterDownloadFile(string message, string sourcePath)
        {
            /*
             * 本函数被执行到，说明文件已经按照存储路径被下载
             * 可利用客户端传过来的 message 扩展出系统自己的文件下载功能
             */
            return Task.CompletedTask;
        }

        Task<string> IFileService.AfterUploadFile(string message, string targetPath)
        {
            /*
             * 本函数被执行到，说明上传文件已经按照写入路径被保存
             * 可利用客户端传过来的 message 扩展出系统自己的文件上传功能
             */

            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为提示用户上传文件已成功保存
             */
            return Task.FromResult(String.Format("上传文件 {0} 已存放在 {1} 目录里", Path.GetFileName(targetPath), Path.GetDirectoryName(targetPath)));
        }

        #endregion
    }
}
