using BepInEx;
using System;
using System.Threading.Tasks;
using System.Globalization;

namespace Straitjacket.Subnautica.Mods.VersionChecker
{
    using ExtensionMethods;
    using NexusAPI;
    using Utility;

    internal class VersionRecord
    {
        public enum VersionState { Unknown, Outdated, Current, Ahead }

        public QModJson QModJson { get; set; }

        private Version currentVersion;
        public virtual Version CurrentVersion => currentVersion switch
        {
            Version version => version,
            _ => Version.TryParse(QModJson.Version, out currentVersion) switch
            {
                true => Version.Parse(currentVersion.ToStringParsed()),
                false => currentVersion = new Version(0, 0, 0, 0)
            }
        };

        private Version latestVersion;
        public Version LatestVersion
        {
            get => latestVersion;
            protected set => latestVersion = Version.Parse(value.ToStringParsed());
        }

        public string Current => CurrentVersion?.ToStringParsed();
        public string Latest => LatestVersion?.ToStringParsed();

        public Game Game => Paths.ProcessName switch
        {
            "Subnautica" when QModJson.NexusId?.Subnautica is string => Game.Subnautica,
            "Subnautica" when QModJson.NexusId?.BelowZero is string => Game.BelowZero,
            "SubnauticaZero" when QModJson.NexusId?.BelowZero is string => Game.BelowZero,
            "SubnauticaZero" when QModJson.NexusId?.Subnautica is string => Game.Subnautica,
            _ => Game.Unknown
        };

        public int NexusModId => Game switch
        {
            Game.Subnautica => int.Parse(QModJson.NexusId?.Subnautica, CultureInfo.InvariantCulture.NumberFormat),
            Game.BelowZero => int.Parse(QModJson.NexusId?.BelowZero, CultureInfo.InvariantCulture.NumberFormat),
            _ => -1
        };

        public string NexusDomainName => Game switch
        {
            Game.Subnautica => "subnautica",
            Game.BelowZero => "subnauticabelowzero",
            _ => throw new InvalidOperationException($"Could not parse Nexus domain name for Game: {Game}")
        };

        public VersionState State => CurrentVersion switch
        {
            Version current when LatestVersion is Version latest && current < latest => VersionState.Outdated,
            Version current when LatestVersion is Version latest && current > latest => VersionState.Ahead,
            Version _ when LatestVersion is Version => VersionState.Current,
            _ => VersionState.Unknown
        };

        public string Prefix => QModJson.Id switch
        {
            Constants.QModId => string.Empty,
            _ => $"[{QModJson.DisplayName}] "
        };

        public VersionRecord(QModJson qModJson)
        {
            QModJson = qModJson;
        }

        public virtual string Message(bool splitLines = false) => State switch
        {
            VersionState.Ahead => $"Currently running v{Current}.{(splitLines ? Environment.NewLine : " ")}" +
                                  $"The latest release version is v{Latest}. We are ahead.",
            VersionState.Current => $"Currently running v{Current}.{(splitLines ? Environment.NewLine : " ")}" +
                                    $"The latest release version is v{Latest}. Up to date.",
            VersionState.Outdated => $"A new version has been released: v{Latest}.{(splitLines ? Environment.NewLine : " ")}" +
                                     $"Currently running v{Current}. Please update at your earliest convenience!",
            _ => "Could not compare versions."
        };

        public virtual void Update() => UpdateAsync().Wait();

        public virtual async Task UpdateAsync()
        {
            if (!QModJson.Enable || (NexusModId < 0 && string.IsNullOrWhiteSpace(QModJson.VersionChecker?.LatestVersionURL)))
            {
                return;
            }

            if (NexusModId < 0 && !string.IsNullOrWhiteSpace(QModJson.VersionChecker?.LatestVersionURL))
            {
                LatestVersion = await GetLatestVersionAsync();
                return;
            }

            LatestVersion = await GetNexusAPILatestVersionAsync();
            if (LatestVersion is Version)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Validate.Main.ApiKey))
            {
                LatestVersion = await GetVersionCheckerAPILatestVersionAsync();
                if (LatestVersion is Version)
                {
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(QModJson.VersionChecker?.LatestVersionURL))
            {
                throw new InvalidOperationException("Could not retrieve version from Nexus Mods API and no VersionChecker github URL is defined as a fallback.");
            }

            LatestVersion = await GetLatestVersionAsync();
        }

        private async Task<Version> GetLatestVersionAsync()
        {
            var json = await Networking.ReadJsonAsync<QModJson>(QModJson.VersionChecker?.LatestVersionURL);
            if (Version.TryParse(json.Version, out Version version))
            {
                return version;
            }
            else
            {
                return null;
            }
        }

        private async Task<Version> GetNexusAPILatestVersionAsync()
        {
            if (string.IsNullOrWhiteSpace(Validate.Main.ApiKey))
            {
                return await GetVersionCheckerAPILatestVersionAsync();
            }

            var json = await ModJson.GetAsync(NexusDomainName, NexusModId, Validate.Main.ApiKey);

            if (json.Available)
            {
                if (Version.TryParse(json.Version, out Version version))
                {
                    return version;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // if the mod is unavailable on nexus mods, return the current version so the user is not constantly
                // bugged to update a mod they can't access
                return CurrentVersion;
            }
        }

        private async Task<Version> GetVersionCheckerAPILatestVersionAsync()
        {
            var json = await VersionCheckerAPI.ModJson.GetAsync(NexusDomainName,
                                                                NexusModId,
                                                                QModJson.DisplayName,
                                                                QModJson.Id);

            if (json.Available)
            {
                if (Version.TryParse(json.Version, out Version version))
                {
                    return version;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // if the mod is unavailable on nexus mods, return the current version so the user is not constantly
                // bugged to update a mod they can't access
                return CurrentVersion;
            }
        }
    }
}
