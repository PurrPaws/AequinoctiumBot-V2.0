﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace AequinoctiumBot
{
    /// <summary>
    /// Character raid progression.
    /// </summary>
    public class Progression
    {
        /// <summary>
        /// Gets or sets progression information for raids.
        /// </summary>
        [JsonProperty("raids")]
        public IList<Raid> Raids { get; set; }
    }
}
