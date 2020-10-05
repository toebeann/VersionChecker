#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Straitjacket.Utility
{
    internal class Networking
    {
        internal static async Task<string> ReadAllTextAsync(string URL, Dictionary<string, string> headers = null)
        {
            using (var client = new WebClient())
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        client.Headers.Add(header.Key, header.Value);
                    }
                }

                return await client.DownloadStringTaskAsync(URL);
            }
        }

        internal static async Task<TJsonObject> ReadJSONAsync<TJsonObject>(string URL, Dictionary<string, string> headers = null)
            where TJsonObject : class
            => JsonConvert.DeserializeObject<TJsonObject>(await ReadAllTextAsync(URL, headers));
    }
}
