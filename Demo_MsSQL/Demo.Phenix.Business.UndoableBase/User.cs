using System;

namespace Demo
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User : Phenix.Core.Security.User<User>
    {
        public override void Activate()
        {
            throw new NotImplementedException();
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }

        public override bool ResetPassword()
        {
            throw new NotImplementedException();
        }

        protected override bool IsValidDynamicPassword(string timestamp, string dynamicPassword, string requestAddress, string requestSession, bool throwIfNotConform = true)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValidPassword(string timestamp, string password, string requestAddress, string requestSession, bool throwIfNotConform = true)
        {
            throw new NotImplementedException();
        }
    }
}