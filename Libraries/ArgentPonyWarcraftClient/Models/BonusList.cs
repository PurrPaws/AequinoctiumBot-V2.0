using Newtonsoft.Json;

namespace AequinoctiumBot
{
    /// <summary>
    /// A bonus list
    /// </summary>
    public class BonusList
    {
        /// <summary>
        /// 
        /// 
        /// 
        /// s or sets the bonus list ID.
        /// </summary>
        [JsonProperty("bonusListId")]
        public int BonusListId { get; set; }

    }
}
