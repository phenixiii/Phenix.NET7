namespace Phenix.Services.Plugin
{
    /// <summary>
    /// WebAPI配置信息
    /// </summary>
    public static class ApiConfig
    {
        /// <summary>
        /// /api/security/gate
        /// </summary>
        public const string ApiSecurityGatePath = "/api/security/gate";

        /// <summary>
        /// /api/security/myself
        /// </summary>
        public const string ApiSecurityMyselfPath = "/api/security/myself";

        /// <summary>
        /// /api/security/myself/company
        /// </summary>
        public const string ApiSecurityMyselfCompanyPath = "/api/security/myself/company";

        /// <summary>
        /// /api/security/myself/company-teams
        /// </summary>
        public const string ApiSecurityMyselfCompanyTeamsPath = "/api/security/myself/company-teams";

        /// <summary>
        /// /api/security/myself/company-user
        /// </summary>
        public const string ApiSecurityMyselfCompanyUserPath = "/api/security/myself/company-user";

        /// <summary>
        /// /api/security/myself/password
        /// </summary>
        public const string ApiSecurityMyselfPasswordPath = "/api/security/myself/password";

        /// <summary>
        /// /api/security/position
        /// </summary>
        public const string ApiSecurityPositionPath = "/api/security/position";

        /// <summary>
        /// /api/security/role
        /// </summary>
        public const string ApiSecurityRolePath = "/api/security/role";

        /// <summary>
        /// /api/security/one-off-key-pair
        /// </summary>
        public const string ApiSecurityOneOffKeyPairPath = "/api/security/one-off-key-pair";

        /// <summary>
        /// /api/data/sequence
        /// </summary>
        public const string ApiDataSequencePath = "/api/data/sequence";

        /// <summary>
        /// /api/data/increment
        /// </summary>
        public const string ApiDataIncrementPath = "/api/data/increment";

        /// <summary>
        /// /api/message/group-message-hub
        /// </summary>
        public const string ApiMessageGroupMessageHubPath = "/api/message/group-message-hub";

        /// <summary>
        /// /api/message/user-message
        /// </summary>
        public const string ApiMessageUserMessagePath = "/api/message/user-message";

        /// <summary>
        /// /api/message/user-message-hub
        /// </summary>
        public const string ApiMessageUserMessageHubPath = "/api/message/user-message-hub";

        /// <summary>
        /// /api/inout/file
        /// </summary>
        public const string ApiInoutFilePath = "/api/inout/file";

        /// <summary>
        /// /api/log/event-log
        /// </summary>
        public const string ApiLogEventLogPath = "/api/log/event-log";

        /// <summary>
        /// /api/service/portal
        /// </summary>
        public const string ApiServicePortalPath = "/api/service/portal";
    }
}