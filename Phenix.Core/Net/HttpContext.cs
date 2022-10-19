using Microsoft.AspNetCore.Http;

namespace Phenix.Core.Net
{
    /// <summary>
    /// ��װ IHttpContextAccessor.HttpContext Ϊ��̬����
    /// ���� HttpContextExtensions ����ʹ��
    /// </summary>
    public static class HttpContext
    {
        #region ����

        private static IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// ��ǰ HttpContext
        /// </summary>
        public static Microsoft.AspNetCore.Http.HttpContext Current
        {
            get { return _contextAccessor.HttpContext; }
        }
        
        #endregion

        #region ����

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        #endregion
    }
}