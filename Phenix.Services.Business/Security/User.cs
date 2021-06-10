using System;
using System.Collections.Generic;
using Phenix.Core.Security;

namespace Phenix.Services.Business.Security
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User : User<User>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private User()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private User(string dataSourceKey,
            long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long rootTeamsId, long teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime,
            string teamsName, string positionName)
            : base(dataSourceKey,
                id, name, phone, eMail, regAlias, regTime,
                requestAddress, requestFailureCount, requestFailureTime,
                rootTeamsId, teamsId, positionId,
                locked, lockedTime, disabled, disabledTime)
        {
            _teamsName = teamsName;
            _positionName = positionName;
        }

        #region 属性

        private string _teamsName;

        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string TeamsName
        {
            get { return _teamsName; }
            private set { _teamsName = value; }
        }

        private string _positionName;

        /// <summary>
        /// 担任岗位名称
        /// </summary>
        public string PositionName
        {
            get { return _positionName; }
            private set { _positionName = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        /// <returns>公司用户资料</returns>
        public IList<User> FetchCompanyUsers(Teams company)
        {
            IDictionary<long, Position> positions = Position.FetchKeyValues(Database, p => p.Id);
            IList<User> result = FetchList(Database, p => p.RootTeamsId == RootTeamsId && p.RootTeamsId != p.TeamsId);
            foreach (User item in result)
            {
                if (item.PositionId.HasValue && positions.TryGetValue(item.PositionId.Value, out Position position))
                    item.PositionName = position.Name;
                Teams teams = company.FindInBranch(p => p.Id == item.TeamsId);
                if (teams != null)
                    item.TeamsName = teams.Name;
            }

            return result;
        }

        #endregion
    }
}