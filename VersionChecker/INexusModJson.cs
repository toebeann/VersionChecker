namespace Straitjacket.Subnautica.Mods.VersionChecker
{
    internal interface INexusModJson
    {
        int ModId { get; }
        int GameId { get; }
        string Name { get; }
        string Version { get; }
        string Status { get; }
        bool Available { get; }
        string DomainName { get; }
    }
}
