namespace CommandQuery.Api.Settings
{
    public class CommandQuerySettings
    {
        /// <summary>
        /// Whether this is the primary global API (and thus has write access)
        /// </summary>
        public bool IsPrimaryApi { get; set; }

        /// <summary>
        /// The FQDN or IP address of the primary URL to make write requests
        /// </summary>
        public string PrimaryApiUrlBase { get; set; }
    }
}
