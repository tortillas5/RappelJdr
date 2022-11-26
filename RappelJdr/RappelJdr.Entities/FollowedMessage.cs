namespace RappelJdr.Entities
{
    /// <summary>
    /// Entity type followed message.
    /// </summary>
    public class FollowedMessage : DefaultEntity
    {
        /// <summary>
        /// Id of the message followed.
        /// </summary>
        public ulong MessageId { get; set; }
    }
}