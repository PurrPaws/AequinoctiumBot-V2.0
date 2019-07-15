using System.Collections.Generic;
using Newtonsoft.Json;

namespace AequinoctiumBot
{
    /// <summary>
    /// An auction files listing.
    /// </summary>
    public class AuctionFiles
    {
        /// <summary>
        /// 
        /// 
        /// s or sets the auction file summaries.
        /// </summary>
        [JsonProperty("files")]
        public IList<AuctionFileSummary> Files { get; set; }
    }
}
