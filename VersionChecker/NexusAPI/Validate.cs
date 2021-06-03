#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.NexusAPI
{
    using Utility;

    internal class Validate : IValidate
    {
        private static Validate main;
        public static Validate Main => main ??= new Validate();

        [JsonRequired, JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "key")]
        public string ApiKey { get; set; } = null;

        [JsonRequired, JsonProperty(PropertyName = "name")]
        public string Username { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "is_premium")]
        public bool IsPremium { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "is_supporter")]
        public bool IsSupporter { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonRequired, JsonProperty(PropertyName = "profile_url")]
        public string ProfileUrl { get; set; }

        private const string Url = "https://api.nexusmods.com/v1/users/validate.json";

        public static async Task<Validate> GetAsync()
            => main = await Networking.ReadJsonAsync<Validate>(Url, new Dictionary<string, string>() { ["apikey"] = Main.ApiKey });
    }
}
