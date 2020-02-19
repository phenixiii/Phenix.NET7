using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;

namespace Demo.InspectionStation.Plugin.Business
{
    /// <summary>
    /// 中控
    /// </summary>
    public class IsCenter : RootEntityBase<IsCenter>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private IsCenter()
        {
            //禁止添加代码
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="name">名称</param>
        public IsCenter(string name)
            : base(Database.Sequence.Value)
        {
            _name = name;
        }

        #region 属性

        #region 基本属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        #endregion

        #region 动态属性

        private ReadOnlyCollection<string> _operationPoints;

        /// <summary>
        /// 监控的作业点
        /// </summary>
        public IList<string> OperationPoints
        {
            get { return _operationPoints; }
        }

        /// <summary>
        /// 监控的作业点
        /// </summary>
        public IDictionary<string, IsOperationPoint> OperationPointDictionary
        {
            get
            {
                Dictionary<string, IsOperationPoint> result = new Dictionary<string, IsOperationPoint>(StringComparer.Ordinal);
                if (_operationPoints != null && _operationPoints.Count > 0)
                    foreach (IsOperationPoint item in IsOperationPoint.Select(p => _operationPoints.Contains(p.Name)))
                        result.Add(item.Name, item);
                return result;
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 监控指定的作业点
        /// </summary>
        /// <param name="operationPoints">作业点</param>
        public void Listen(IList<string> operationPoints)
        {
            UpdateSelf(SetProperty(p => p.OperationPoints, operationPoints));
        }

        private static void Initialize(Database database)
        {
            database.ExecuteNonQuery(@"
CREATE TABLE IS_Center (
  IC_ID NUMERIC(15) NOT NULL,
  IC_Name VARCHAR(100) NOT NULL,
  IC_Operation_Points VARCHAR(4000) NOT NULL,
  PRIMARY KEY(IC_ID),
  UNIQUE(IC_Name)
)", false);
        }

        #endregion
    }
}