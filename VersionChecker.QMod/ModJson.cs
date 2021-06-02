#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    /// <summary>
    /// A bare-bones definition of a mod.json file for retrieving the Version property.
    /// </summary>
    public class ModJson
    {
        /// <summary>
        /// The Version property containing the version number.
        /// </summary>
        [JsonRequired]
        public string Version { get; set; }

        /// <summary>
        /// The DisplayName property containing the display name of the mod.
        /// </summary>
        [JsonRequired]
        public string DisplayName { get; set; }

        /// <summary>
        /// Unique mod ID.
        /// </summary>
        [JsonRequired]
        public string Id { get; set; }

        /// <summary>
        /// Whether the mod is enabled.
        /// </summary>
        [JsonProperty]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// An object containing properties for VersionChecker.
        /// </summary>
        [JsonProperty]
        public VersionCheckerOptions VersionChecker { get; set; } = null;

        /// <summary>
        /// An object containing Nexus Mods ids for VersionChecker, SAM etc.
        /// </summary>
        [JsonProperty]
        public NexusIdOptions NexusId { get; set; } = null;

        /// <summary>
        /// VersionCheckerOptions class describing the properties belonging to the VersionChecker mod.json object.
        /// </summary>
        public class VersionCheckerOptions
        {
            /// <summary>
            /// Where VersionChecker can find the latest copy of this mod.json online.
            /// eg. "https://github.com/my-github/my-mod/raw/master/mod.json".
            /// </summary>
            [JsonRequired]
            public string LatestVersionURL { get; set; }
        }

        /// <summary>
        /// NexusIdOptions class describing the properties belonging to the NexusId mod.json object.
        /// </summary>
        public class NexusIdOptions
        {
            /// <summary>
            /// Nexus Mods Id for the game on the Subnautica Nexus Mods site.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string Subnautica { get; set; } = null;

            /// <summary>
            /// Nexus Mods Id for the game on the Subnautica: Below Zero Nexus Mods site.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string BelowZero { get; set; } = null;
        }
    }
}
