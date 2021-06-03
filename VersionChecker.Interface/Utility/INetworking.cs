using System.Collections.Generic;
using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.Interface.Utility
{
    internal interface INetworking
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public Task<string> ReadAllTextAsync();
        public Task<TJsonObject> ReadJsonAsync<TJsonObject>() where TJsonObject : class;

        public string ReadAllText();
        public TJsonObject ReadJson<TJsonObject>() where TJsonObject : class;
    }
}
