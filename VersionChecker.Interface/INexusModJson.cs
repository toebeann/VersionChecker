using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.Interface
{
    internal interface INexusModJson
    {
        public int ModId { get; }
        public int GameId { get; }
        public string Name { get; }
        public string Version { get; }
        public string Status { get; }
        public bool Available { get; }
        public string DomainName { get; }

        public void Update();
        public Task UpdateAsync();
    }
}
