#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System;
using System.Net;
using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.VersionCheckerAPI
{
    using Utility;

    internal class ModJson : MarshalByRefObject, INexusModJson
    {
        [JsonRequired, JsonProperty(PropertyName = "mod_id")]
        public int ModId { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "game_id")]
        public int GameId { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "available")]
        public bool Available { get; set; }

        [JsonProperty(PropertyName = "domain_name")]
        public string DomainName { get; set; }

        public void Update() => UpdateAsync().Wait();

        public async Task UpdateAsync()
        {
            var modJson = await GetAsync(DomainName, ModId, Name);
            ModId = modJson.ModId;
            GameId = modJson.GameId;
            Name = modJson.Name;
            Version = modJson.Version;
            Status = modJson.Status;
            Available = modJson.Available;
        }

        public static ModJson Get(string domain, int id, string name)
            => GetAsync(domain, id, name).Result;

        public static async Task<ModJson> GetAsync(string domain, int id, string name)
            => await Networking.ReadJsonAsync<ModJson>(GetUrl(domain, id, name));

        private static string GetUrl(string domain, int id, string name)
            => "http://mods.vc.api.straitjacket.software/v1/" +
               $"games/{WebUtility.UrlEncode(domain)}/" +
               $"mods/{WebUtility.UrlEncode(id.ToString())}" +
               $"{WebUtility.UrlEncode(name)}.json";
    }
}
