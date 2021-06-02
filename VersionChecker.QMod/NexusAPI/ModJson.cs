#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod.NexusAPI
{
    internal class ModJson
    {
        [JsonRequired, JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "mod_id")]
        public int ModId { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "game_id")]
        public int GameId { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "domain_name")]
        public string DomainName { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "available")]
        public bool Available { get; set; }
    }
}
