#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif

namespace Straitjacket.Subnautica.Mods.VersionChecker
{
    using Interface;

    internal class QModJson : IQModJson
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

        IQModJson.IVersionCheckerOptions IQModJson.VersionChecker => VersionChecker;

        IQModJson.INexusIdOptions IQModJson.NexusId => NexusId;

        public class VersionCheckerOptions : IQModJson.IVersionCheckerOptions
        {
            [JsonRequired]
            public string LatestVersionURL { get; set; }
        }

        public class NexusIdOptions : IQModJson.INexusIdOptions
        {
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string Subnautica { get; set; } = null;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string BelowZero { get; set; } = null;
        }
    }
}
