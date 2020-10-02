#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System.Net;
using System.Threading.Tasks;

namespace Straitjacket.Utility
{
    internal class Networking
    {
        private const string GOOGLE_204_URL = "http://google.com/generate_204";
        internal static async Task<bool> CheckConnectionAsync(string URL = GOOGLE_204_URL)
        {
            if (URL != GOOGLE_204_URL)
            {
                var preliminary = await CheckConnectionAsync(GOOGLE_204_URL);
                if (preliminary)
                {
                    return preliminary;
                }
            }

            try
            {
                using (var client = new WebClient())
                using (var stream = await client.OpenReadTaskAsync(URL))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static async Task<string> ReadAllTextAsync(string URL)
        {
            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(URL);
            }
        }

        internal static async Task<TJsonObject> ReadJSONAsync<TJsonObject>(string URL) where TJsonObject : class
            => JsonConvert.DeserializeObject<TJsonObject>(await ReadAllTextAsync(URL));
    }
}
