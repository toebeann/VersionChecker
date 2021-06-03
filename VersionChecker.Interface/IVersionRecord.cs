using System;

namespace Straitjacket.Subnautica.Mods.VersionChecker.Interface
{
    internal interface IVersionRecord : IUpdatable, IUpdatableAsync
    {
        public enum VersionState { Unknown, Outdated, Current, Ahead }

        IQModJson QModJson { get; }
        Version CurrentVersion { get; }
        Version LatestVersion { get; }
        string Current { get; }
        string Latest { get; }
        Game Game { get; }
        int NexusModId { get; }
        string NexusDomainName { get; }
        VersionState State { get; }
        string Prefix { get; }

        string Message(bool splitLines);
    }
}
