namespace RappelJdr.Entities
{
    /// <summary>
    /// Entity type role.
    /// </summary>
    public class Role : DefaultEntity
    {
        /// <summary>
        /// Emoji linked to the role.
        /// </summary>
        public string Emoji { get; set; }

        /// <summary>
        /// Name of the role.
        /// </summary>
        public string Name { get; set; }
    }
}