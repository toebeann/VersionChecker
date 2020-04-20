using System.Net;
using Oculus.Newtonsoft.Json;

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

        internal static T ReadJSON<T>(string URL) where T : class
        {
            var text = ReadAllText(URL);
            return JsonConvert.DeserializeObject<T>(text);
        }

        internal static bool TryReadJSON<T>(string URL, out T JSON) where T : class
        {
            try
            {
                JSON = ReadJSON<T>(URL);
                return true;
            }
            catch
            {
                JSON = null;
                return false;
            }
        }
    }
}
