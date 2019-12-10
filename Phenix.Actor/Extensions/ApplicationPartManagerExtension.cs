using System;
using System.IO;
using System.Reflection;
using Orleans.ApplicationParts;
using Phenix.Core;

namespace Orleans.Hosting
{
    /// <summary>
    /// IApplicationPartManager扩展
    /// </summary>
    public static class ApplicationPartManagerExtension
    {
        /// <summary>
        /// 装配Actor插件
        /// </summary>
        /// <param name="manager">IApplicationPartManager</param>
        /// <param name="filter">装配的Actor插件所在程序集统一采用"*.Plugin.dll"作为文件名后缀</param>
        public static void AddPluginPart(this IApplicationPartManager manager, string filter = "*.Plugin.dll")
        {
            if (String.IsNullOrEmpty(filter))
                throw new ArgumentNullException(nameof(filter));

            foreach (string fileName in Directory.GetFiles(AppRun.BaseDirectory, filter))
                manager.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
        }
    }
}
