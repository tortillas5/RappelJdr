namespace RappelJdr
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the token of the bot.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Current value of the token.
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Value { get; set; }
    }
}