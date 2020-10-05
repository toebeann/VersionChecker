#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using QModManager.API;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Straitjacket.Utility.VersionChecker
{
    internal class VersionRecord
    {
        public enum VersionState { Unknown, Outdated, Current, Ahead }

        public Assembly Assembly;
        public string DisplayName;
        public Color Colour;
        public string URL;
        public Version CurrentVersion;
        public Version LatestVersion;
        public QModGame Game = QModGame.None;
        public uint ModId;
        public Func<Task> UpdateAsync;

        public string NexusDomainName => Game switch
        {
            QModGame.Subnautica => "subnautica",
            QModGame.BelowZero => "subnauticabelowzero",
            _ => throw new InvalidOperationException($"Could not get Nexus domain name for QModGame: {Game}")
        };
        public string NexusAPIModUrl => Game == QModGame.None || ModId == 0
            ? null
            : $"https://api.nexusmods.com/v1/games/{NexusDomainName}/mods/{ModId}.json";
        public string VersionCheckerAPIModUrl => Game == QModGame.None || ModId == 0
            ? null
            : $"https://mods.vc.api.straitjacket.software/v1/games/{NexusDomainName}/mods/{ModId}.json";

        public VersionState State
        {
            get
            {
                if (CurrentVersion == null || LatestVersion == null || CurrentVersion == null || LatestVersion == null)
                {
                    return VersionState.Unknown;
                }
                else if (CurrentVersion < LatestVersion)
                {
                    return VersionState.Outdated;
                }
                else if (CurrentVersion > LatestVersion)
                {
                    return VersionState.Ahead;
                }
                else
                {
                    return VersionState.Current;
                }
            }
        }

        public async Task UpdateLatestVersionAsync()
        {
            if (UpdateAsync != null)
            {
                string prefix = Assembly == Assembly.GetAssembly(typeof(VersionChecker))
                    ? string.Empty
                    : $"[{DisplayName}] ";

                try
                {
                    await UpdateAsync();
                }
                catch (WebException e)
                {
                    VersionChecker.Main.LogErrors.Add($"{prefix}There was an error retrieving the latest version: " +
                        $"Could not connect to address {URL}");
                    VersionChecker.Main.LogErrors.Add(e.Message);
                }
                catch (JsonReaderException e)
                {
                    VersionChecker.Main.LogErrors.Add($"{prefix}There was an error retrieving the latest version: " +
                        $"Invalid JSON found at address {URL}");
                    VersionChecker.Main.LogErrors.Add(e.Message);
                }
                catch (JsonSerializationException e)
                {
                    VersionChecker.Main.LogErrors.Add($"{prefix}There was an error retrieving the latest version:");
                    VersionChecker.Main.LogErrors.Add(e.Message);
                }
                catch (InvalidOperationException e)
                {
                    VersionChecker.Main.LogErrors.Add($"{prefix}There was an error retrieving the latest version:");
                    VersionChecker.Main.LogErrors.Add(e.Message);
                }
                catch (Exception e)
                {
                    VersionChecker.Main.LogErrors.Add($"{prefix}There was an unhandled error retrieving the latest version.");
                    VersionChecker.Main.LogErrors.Add(e.ToString());
                }
            }
        }

        public string Message(bool splitLines = false)
        {
            switch (State)
            {
                case VersionState.Ahead:
                    return $"Currently running v{CurrentVersion}." +
                    (splitLines ? Environment.NewLine : " ") +
                    $"The latest release version is v{LatestVersion}. " +
                    "We are ahead.";
                case VersionState.Current:
                    return $"Currently running v{CurrentVersion}." +
                        (splitLines ? Environment.NewLine : " ") +
                        $"The latest release version is v{LatestVersion}. " +
                        "Up to date.";
                case VersionState.Outdated:
                    return $"A new version has been released: v{LatestVersion}." +
                    (splitLines ? Environment.NewLine : " ") +
                    $"Currently running v{CurrentVersion}. " +
                    "Please update at your earliest convenience!";
                case VersionState.Unknown:
                default:
                    return "Could not compare versions.";
            }
        }
    }
}
