namespace TortillasEntities
{
    using System;

    /// <summary>
    /// Entity type Session.
    /// </summary>
    public class Session : DefaultEntity
    {
        public ulong ChannelId { get; set; }

        public DateTime Date { get; set; }

        public string Name { get; set; }

        public ulong ServerId { get; set; }
    }
}