using System;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Phenix.Core.Data;
using Phenix.Mapper.Expressions;
using Phenix.Mapper.Schema;
using Phenix.Core.Security;

namespace Phenix.Net.Api
{
    /// <summary>
    /// 控制器基类
    /// </summary>
    [EnableCors]
    public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        #region 属性

        /// <summary>
        /// 用户身份
        /// </summary>
        public new Principal User
        {
            get { return Principal.CurrentPrincipal; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// 对应 phAjax.decrypt 函数
        /// </summary>
        /// <param name="sourceData">需加密的对象/字符串</param>
        protected async Task<string> EncryptAsync(object sourceData)
        {
            if (User.Identity == null)
                throw new AuthenticationException();

            return await User.Identity.Encrypt(sourceData);
        }

        #region SelectRecord

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<T>> SelectRecord<T>(Database database, Expression<Func<T, bool>> criteriaLambda, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
        {
            return SelectRecord<T, T>(database, criteriaLambda, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<T>> SelectRecord<T>(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
        {
            return SelectRecord<T, T>(database, criteriaLambda, criteria, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<T>> SelectRecord<T>(Database database, CriteriaExpression criteriaExpression, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
        {
            return SelectRecord<T, T>(database, criteriaExpression, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<T>> SelectRecord<T>(Database database, CriteriaExpression criteriaExpression, object criteria, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
        {
            return SelectRecord<T, T>(database, criteriaExpression, criteria, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<TSub>> SelectRecord<T, TSub>(Database database, Expression<Func<T, bool>> criteriaLambda, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
            where TSub : class
        {
            return SelectRecord<T, TSub>(database, criteriaLambda, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<TSub>> SelectRecord<T, TSub>(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
            where TSub : class
        {
            return SelectRecord<T, TSub>(database, CriteriaExpression.Where(criteriaLambda), criteria, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<TSub>> SelectRecord<T, TSub>(Database database, CriteriaExpression criteriaExpression, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
            where TSub : class
        {
            return SelectRecord<T, TSub>(database, criteriaExpression, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>DataPageInfo</returns>
        protected Task<DataPageInfo<TSub>> SelectRecord<T, TSub>(Database database, CriteriaExpression criteriaExpression, object criteria, int pageNo, int pageSize = 10, params OrderBy<T>[] orderBys)
            where T : class
            where TSub : class
        {
            Sheet sheet = MetaData.Fetch(database).FindSheet<T>(true);
            return Task.FromResult(new DataPageInfo<TSub>(sheet.RecordCount(criteriaExpression, criteria), pageNo, pageSize, sheet.SelectRecord<T, TSub>(criteriaExpression, criteria, pageNo, pageSize, orderBys)));
        }

        #endregion

        #endregion
    }
}
