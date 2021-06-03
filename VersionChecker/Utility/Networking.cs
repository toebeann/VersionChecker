#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.Utility
{
    internal class Networking
    {
        public static string ReadAllText(string url, Dictionary<string, string> headers = null)
            => ReadAllTextAsync(url, headers).Result;

        public static TJsonObject ReadJson<TJsonObject>(string url, Dictionary<string, string> headers = null)
            where TJsonObject : class
            => ReadJsonAsync<TJsonObject>(url, headers).Result;

        public static async Task<string> ReadAllTextAsync(string url, Dictionary<string, string> headers = null)
        {
            using var client = new WebClient();
            if (headers is Dictionary<string, string>)
            {
                foreach (var header in headers)
                {
                    client.Headers.Add(header.Key, header.Value);
                }
            }

            return await client.DownloadStringTaskAsync(url);
        }

        public static async Task<TJsonObject> ReadJsonAsync<TJsonObject>(string url, Dictionary<string, string> headers = null)
            where TJsonObject : class
            => JsonConvert.DeserializeObject<TJsonObject>(await ReadAllTextAsync(url, headers));
    }
}
