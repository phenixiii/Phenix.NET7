namespace Phenix.Actor
{
    internal static class ContextKeys
    {
        /// <summary>
        /// CurrentIdentity.CompanyName
        /// </summary>
        public const string CurrentIdentityCompanyName = "CurrentIdentity.CompanyName";

        /// <summary>
        /// CurrentIdentity.UserName
        /// </summary>
        public const string CurrentIdentityUserName = "CurrentIdentity.UserName";

        /// <summary>
        /// CurrentIdentity.CultureName
        /// </summary>
        public const string CurrentIdentityCultureName = "CurrentIdentity.CultureName";

        /// <summary>
        /// Trace.Key
        /// </summary>
        public const string TraceKey = "Trace.Key";

        /// <summary>
        /// Trace.Order
        /// </summary>
        public const string TraceOrder = "Trace.Order";

        /// <summary>
        /// 遵循FIFO原则的SimpleMessageStream提供者名称
        /// </summary>
        public const string SimpleMessageStreamProviderName = "SMSProvider";
    }
}
