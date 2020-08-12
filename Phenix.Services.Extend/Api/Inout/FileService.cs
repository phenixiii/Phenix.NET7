using System;
using System.IO;
using Phenix.Core.IO;

namespace Phenix.Services.Extend.Api.Inout
{
    /// <summary>
    /// 文件存取服务
    /// </summary>
    public sealed class FileService : IFileService
    {
        #region 方法

        /// <summary>
        /// 获取上传文件的写入路径
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="fileName">上传文件名</param>
        /// <returns>写入路径</returns>
        string IFileService.GetUploadPath(string message, string fileName)
        {
            return null; //默认为 Phenix.Core.AppRun.TempDirectory + fileName
        }

        /// <summary>
        /// 当完成上传文件时触发
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="targetPath">写入路径</param>
        /// <returns>完成上传时返回消息</returns>
        string IFileService.AfterUploadFile(string message, string targetPath)
        {
            /*
             * 本函数被执行到，说明上传文件已经按照写入路径被保存
             * 可利用客户端传过来的 message 扩展出系统自己的文件上传功能
             */

            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为提示用户上传文件已成功保存
             */
            return String.Format("上传文件 {0} 已存放在 {1} 目录里", Path.GetFileName(targetPath), Path.GetDirectoryName(targetPath));
        }

        /// <summary>
        /// 获取下载文件的读取路径
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="fileName">下载文件名</param>
        /// <returns>读取路径</returns>
        string IFileService.GetDownloadPath(string message, string fileName)
        {
            return null; //默认为 Phenix.Core.AppRun.TempDirectory + fileName
        }

        /// <summary>
        /// 当完成下载文件时触发
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="sourcePath">读取路径</param>
        void IFileService.AfterDownloadFile(string message, string sourcePath)
        {
            /*
             * 本函数被执行到，说明文件已经按照存储路径被下载
             * 可利用客户端传过来的 message 扩展出系统自己的文件下载功能
             */
        }

        #endregion
    }
}
