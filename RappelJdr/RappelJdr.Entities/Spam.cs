namespace RappelJdr.Entities
{
    /// <summary>
    /// Entity type Spam.
    /// </summary>
    public class Spam : DefaultEntity
    {
        /// <summary>
        /// Id of the channel's spam.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Spam to send.
        /// </summary>
        public string Message { get; set; }
    }
}