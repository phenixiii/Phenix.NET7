using System;

namespace Demo
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User : Phenix.Core.Security.User<User>
    {
        public override void ResetPassword()
        {
            throw new NotImplementedException();
        }

        public override void ChangePassword(string newPassword)
        {
            throw new NotImplementedException();
        }

        public override void Activate()
        {
            throw new NotImplementedException();
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }
        
    }
}