#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System.Net;

namespace Straitjacket.Utility
{
    internal class Networking
    {
        private const string GOOGLE_204_URL = "http://google.com/generate_204";
        internal static bool CheckConnection(string URL = GOOGLE_204_URL)
        {
            if (URL != GOOGLE_204_URL)
            {
                var preliminary = CheckConnection(GOOGLE_204_URL);
                if (preliminary)
                {
                    return preliminary;
                }
            }

            try
            {
                using (var client = new WebClient())
                using (client.OpenRead(URL))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static string ReadAllText(string URL)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(URL);
            }
        }
        internal static bool TryReadAllText(string URL, out string text)
        {
            try
            {
                text = ReadAllText(URL);
                return true;
            }
            catch
            {
                text = null;
                return false;
            }
        }

        internal static TJsonObject ReadJSON<TJsonObject>(string URL) where TJsonObject : class
            => JsonConvert.DeserializeObject<TJsonObject>(ReadAllText(URL));

        internal static bool TryReadJSON<TJsonObject>(string URL, out TJsonObject JsonObject) where TJsonObject : class
        {
            try
            {
                JsonObject = ReadJSON<TJsonObject>(URL);
                return true;
            }
            catch
            {
                JsonObject = null;
                return false;
            }
        }
    }
}
