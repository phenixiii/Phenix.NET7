using System;
using System.IO;
using Phenix.Core.Data.Model;
using Phenix.Core.Net.Api;

namespace Phenix.Services
{
    /// <summary>
    /// 文件存取服务
    /// </summary>
    public class FileService : IFileService
    {
        #region 方法

        /// <summary>
        /// 获取上传文件存储路径
        /// </summary>
        /// <param name="data">客户端动态实体</param>
        /// <param name="fileName">上传文件名</param>
        public string GetUploadPath(DynamicEntity data, string fileName)
        {
            return null; //默认为 Phenix.Core.AppRun.TempDirectory + fileName
        }

        /// <summary>
        /// 当完成上传文件时触发
        /// </summary>
        /// <param name="data">客户端动态实体</param>
        /// <param name="path">文件存储路径</param>
        /// <returns>返回的数据</returns>
        public object AfterUploadFile(DynamicEntity data, string path)
        {
            /*
             * 本函数被执行到，说明上传文件已经按照路径被存储
             * 可利用客户端传过来的 data 以及文件存储路径 path 扩展出系统自己的上传文件功能
             */

            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为提示用户上传文件已成功保存
             */
            return String.Format("上传文件 {0} 已存放在 {1} 目录里", Path.GetFileName(path), Path.GetDirectoryName(path));
        }

        /// <summary>
        /// 获取下载文件存储路径
        /// </summary>
        /// <param name="data">客户端动态实体</param>
        /// <param name="fileName">下载文件名</param>
        public string GetDownloadPath(DynamicEntity data, string fileName)
        {
            return null; //默认为 Phenix.Core.AppRun.TempDirectory + fileName
        }
        
        /// <summary>
        /// 当完成下载文件时触发
        /// </summary>
        /// <param name="data">客户端动态实体</param>
        /// <param name="path">文件存储路径</param>
        public void AfterDownloadFile(DynamicEntity data, string path)
        {
            /*
             * 本函数被执行到，说明下载文件已经按照路径被下载
             * 可利用客户端传过来的 data 以及文件存储路径 path 扩展出系统自己的下载文件功能
             */
        }

        #endregion
    }
}
