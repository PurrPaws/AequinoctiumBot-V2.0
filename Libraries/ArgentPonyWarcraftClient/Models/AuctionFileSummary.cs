﻿using System;
using Newtonsoft.Json;

namespace AequinoctiumBot
{
    /// <summary>
    /// An auction file.
    /// </summary>
    public class AuctionFileSummary
    {
        /// <summary>
        /// 
        /// 
        /// s or sets the URL for the JSON-formatted auction data file.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the last modified timestamp.
        /// </summary>
        [JsonProperty("lastModified")]
        public DateTime LastModified { get; set; }
    }
}
