namespace Straitjacket.Subnautica.Mods.VersionChecker.Interface
{
    internal interface IQModJson
    {
        public string Version { get; }
        public string DisplayName { get; }
        public string Id { get; }
        public bool Enable { get; }
        public IVersionCheckerOptions VersionChecker { get; }
        public INexusIdOptions NexusId { get; }

        public interface IVersionCheckerOptions
        {
            public string LatestVersionURL { get; }
        }

        public interface INexusIdOptions
        {
            public string Subnautica { get; }
            public string BelowZero { get; }
        }
    }
}
