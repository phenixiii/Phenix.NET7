using System;
using System.Linq.Expressions;
using Phenix.Core.Data.Expressions;

namespace Phenix.Client.DataModel
{
    /// <summary>
    /// 数据基类
    /// </summary>
    [Serializable]
    public abstract class DataBase<T>
        where T : DataBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected DataBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected DataBase(string dataSourceKey)
        {
            _dataSourceKey = dataSourceKey;
        }

        #region 属性

        private string _dataSourceKey;

        /// <summary>
        /// 数据源键
        /// </summary>
        public string DataSourceKey
        {
            get { return _dataSourceKey; }
        }

        #endregion

        #region 方法

        #region Property

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="value">值</param>
        public static NameValue<T> Set(Expression<Func<T, object>> propertyLambda, object value)
        {
            return NameValue.Set(propertyLambda, value);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="valueLambda">值 lambda 表达式</param>
        public static NameValue<T> Set(Expression<Func<T, object>> propertyLambda, Expression<Func<T, object>> valueLambda)
        {
            return NameValue.Set(propertyLambda, valueLambda);
        }

        #endregion

        #endregion
    }
}