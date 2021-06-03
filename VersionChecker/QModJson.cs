#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif

namespace Straitjacket.Subnautica.Mods.VersionChecker
{
    internal class QModJson
    {
        [JsonRequired]
        public string Version { get; set; }

        [JsonRequired]
        public string DisplayName { get; set; }

        [JsonRequired]
        public string Id { get; set; }

        [JsonProperty]
        public bool Enable { get; set; } = true;

        [JsonProperty]
        public VersionCheckerOptions VersionChecker { get; set; } = null;

        [JsonProperty]
        public NexusIdOptions NexusId { get; set; } = null;

        public class VersionCheckerOptions
        {
            [JsonRequired]
            public string LatestVersionURL { get; set; }
        }

        public class NexusIdOptions
        {
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string Subnautica { get; set; } = null;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string BelowZero { get; set; } = null;
        }
    }
}
