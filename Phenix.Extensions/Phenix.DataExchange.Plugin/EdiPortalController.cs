using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
#if MySQL
#endif
#if ORA
using Oracle.ManagedDataAccess.Client;
#endif
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;
using Phenix.Core.Net;
using Phenix.Core.Reflection;

namespace Phenix.DataExchange.Plugin
{
    /// <summary>
    /// EDI数据门户控制器
    /// </summary>
    [EnableCors]
    /*
     * 申明是公共服务
     * 生产环境下请根据需要取舍
     */
    [AllowAnonymous]
    /*
     * 一个控制器匹配N个路由，响应对不同资源的请求
     * 以下[Route]仅为示例，生产环境下请根据需要覆写它们，或者采取在运行期动态添加路由的办法
     */
    [Route("/api/data/edi/weather")]
    [Route("/api/data/edi/vessel")]
    [ApiController]
    public class EdiPortalController : Phenix.Core.Net.ControllerBase
    {
        #region 属性

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected virtual Database Database
        {
            get
            {
                /*
                 * 申明是缺省数据库
                 * 生产环境下可自己制定一套规则，动态提供不同的数据库
                 */
                return Database.Default;
            }
        }

#if MySQL
        /// <summary>
        /// 读取的表名
        /// </summary>
        protected virtual string ReadTableName
        {
            get
            {
                /*
                 * 本示例将 Path 转译为待读取的表名
                 * 比如将"/api/data/edi/weather"转译为"edi_weather"
                 * 比如将"/api/data/edi/vessel"转译为"edi_vessel"
                 * 生产环境下可自己制定一套规则进行转译
                */
                return Request.Path.Value.Substring(0, 10).Replace('/', '_');
            }
        }

        /// <summary>
        /// 读取的表
        /// </summary>
        protected Table ReadTable
        {
            get { return Database.MetaData.FindTable(ReadTableName, true); }
        }
#endif
#if ORA
        /// <summary>
        /// 读取数据的存储过程名
        /// </summary>
        protected virtual string ReaderName
        {
            get
            {
                /*
                 * 本示例将 Path 转译为存储过程名
                 * 比如将"/api/data/edi/weather"转译为"edi.weather"
                 * 比如将"/api/data/edi/vessel"转译为"edi.vessel"
                 * 生产环境下可自己制定一套规则进行转译
                 */
                return Request.Path.Value.Substring(0, 10).Replace('/', '.');
            }
        }
#endif

        /// <summary>
        /// 写入的表名
        /// </summary>
        protected virtual string WriteTableName
        {
            get
            {
                /*
                 * 本示例将 Path 转译为待写入的表名
                 * 比如将"/api/data/edi/weather"转译为"edi_weather"
                 * 比如将"/api/data/edi/vessel"转译为"edi_vessel"
                 * 生产环境下可自己制定一套规则进行转译
                */
                return Request.Path.Value.Substring(0, 10).Replace('/', '_');
            }
        }

        /// <summary>
        /// 写入的表
        /// </summary>
        protected Table WriteTable
        {
            get
            {
                /*
                 * 收录报文数据的表，有多少种报文就建多少个表，一一对应，作为报文处理的缓冲桶
                 * 本示例仅处理单层结构报文，如果报文是树状结构，可转换为冗余的单层结构，然后再保存到表中
                 * 表结构请事先在数据库中构造，表字段的命名，应与客户端提交报文体的"属性名-属性值"键值队列中的属性名保持映射关系
                 */
                return Database.MetaData.FindTable(WriteTableName, true);
            }
        }

        #endregion

        #region 方法

#if MySQL
        /// <summary>
        /// 读取数据
        /// </summary>
        [HttpGet]
        public string SelectRecord()
        {

            /*
             * 报文发送，建议用 push，但 pull 也是可行的，那就在这里操作 ReadTable 指向的表，注意读取过程的状态控制（隐含对并发的处理），避免重复读取或出现遗漏
             * 公网环境下，不建议开发通用的查询功能，因为势必要把查询条件的规则拱手交给客户端掌控，数据安全性上是无法得到保障的
             * 一个控制器下编写N个存储过程响应N种查询请求是一个可行的解决方案，但 MySQL 对存储过程支持较弱，所以要编写N个控制器响应N种查询请求，本质上没什么区别
             */
            throw new NotImplementedException();
        }
#endif
#if ORA
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>返回JSON格式的记录</returns>
        [HttpGet]
        public string SelectRecord()
        {
            /*
             * 传入参数为报文体的"参数名-参数值"键值队列
             * 鉴于 Oracle.DataAccess 传参对顺序敏感，对参数名不敏感，所以客户端只要传正确顺序的参数值即可
             */
            return Database.ExecuteGet(SelectRecord, Utilities.JsonDeserialize<IDictionary<string, object>>(Request.ReadBodyAsString()));
        }

        private string SelectRecord(DbConnection connection, IDictionary<string, object> paramValues)
        {
            /*
             * 调用 ReaderName 指定的存储过程
             */
            using (DbCommand command = DbCommandHelper.CreateStoredProc(connection, ReaderName))
            {
                /*
                 * Oracle.DataAccess 默认是按照顺序传参的，本写法最后一个参数是游标，被调用的存储过程都应该要求最后一个参数是 out 的游标
                 * 传参的参数名，对于 Oracle.DataAccess 是毫无意义的，在代码里仅起到用参数名找到参数对象的作用，比如"pCursor"
                 * 传参顺序务必和存储过程的参数排列顺序一致，也就是说，要和客户端约定好提交的报文体"参数名-参数值"键值队列
                 */
                DbCommandHelper.CreateParameter(command, paramValues);
                DbCommandHelper.CreateParameter(command, "pCursor", OracleDbType.RefCursor, ParameterDirection.Output);
                DbCommandHelper.ExecuteNonQuery(command);
                object cursor = command.Parameters["pCursor"].Value;
                if (cursor != DBNull.Value)
                {
                    using (DataReader reader = new DataReader((DbDataReader) cursor))
                    {
                        /*
                         * 返回JSON格式的记录
                         * 属性名依照数据源设定的别名，要和客户端约定一致，以便对方能够正确解析
                         */
                        return reader.SelectRecord();
                    }
                }

                return null;
            }
        }
#endif

        /// <summary>
        /// 接收报文
        /// </summary>
        [HttpPost]
        public void Receive()
        {
            /*
             * 传入参数为报文体的"属性名-属性值"键值队列
             * 根据传入的键值对，用属性名匹配到表字段写入属性值
             * 接收报文时，先一股脑收下，然后再异步一个个处理它们（所以完全可以保存原始结构的报文）
             * 异步处理报文（本示例是已被写入的表记录）时，如果需要反馈消息给到发送方，也是通过异步方式
             */
            WriteTable.InsertRecord(Utilities.JsonDeserialize<IDictionary<string, object>>(Request.ReadBodyAsString()));
        }

        #endregion
    }
}