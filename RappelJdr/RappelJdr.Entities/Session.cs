namespace TortillasEntities
{
    using System;

    /// <summary>
    /// Entity type Session.
    /// </summary>
    public class Session : DefaultEntity
    {
        public DateTime Date { get; set; }

        public string Name { get; set; }

        public ulong ServerId{ get; set; }

        public ulong ChannelId { get; set; }
    }
}