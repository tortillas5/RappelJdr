namespace RappelJdr.Entities
{
    using System;

    /// <summary>
    /// Entity type Session.
    /// </summary>
    public class Session : DefaultEntity
    {
        /// <summary>
        /// Id of the channel's session.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Date of the session.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Name of the session.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the server's session.
        /// </summary>
        public ulong ServerId { get; set; }
    }
}