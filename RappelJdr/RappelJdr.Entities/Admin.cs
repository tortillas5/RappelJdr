namespace RappelJdr.Entities
{
    using TortillasEntities;

    /// <summary>
    /// Entity type admin.
    /// </summary>
    public class Admin : DefaultEntity
    {
        /// <summary>
        /// Username of the admin.
        /// </summary>
        public string Name { get; set; }
    }
}