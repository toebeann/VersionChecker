#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using QModManager.API;
using QModManager.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    using Utility;

    internal class VersionRecord
    {
        public enum VersionState { Unknown, Outdated, Current, Ahead }

        private static readonly Color[] assemblyColours = new Color[]
        {
            new Color(0, 1, 1), new Color(.65f, .165f, .165f), new Color(0, .5f, 0), new Color(.68f, .85f, .9f), new Color(0, .5f, .5f),
            new Color(0, 1, 0), new Color(1, 0, 1), new Color(.5f, .5f, 0), new Color(1, .65f, 0), new Color(1, 1, 0)
        };
        private static readonly List<Color> assignedColours = new List<Color>();
        public static Color GetColour()
        {
            var availableColours = assemblyColours.Except(assignedColours).Union(assignedColours.Except(assignedColours));
            if (!availableColours.Any())
            {
                var result = assignedColours.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
                while (result.Max(x => x.Value) != result.Min(x => x.Value))
                {
                    result = result.Except(result.Where(x => x.Value == result.Max(y => y.Value)))
                        .ToDictionary(x => x.Key, x => x.Value);
                }
                availableColours = result.Keys.Distinct();
            }

            var colour = availableColours.ElementAt(UnityEngine.Random.Range(0, availableColours.Count() - 1));
            assignedColours.Add(colour);
            return colour;
        }

        private static readonly VersionParser VersionParser = new VersionParser();

        public Assembly Assembly => QMod.LoadedAssembly;
        public string DisplayName => QMod.DisplayName;
        private string DisplayNameEncoded => WebUtility.UrlEncode(DisplayName);

        private Color? colour;
        public Color Colour => colour ??= GetColour();

        public string URL { get; }
        public Version CurrentVersion => QMod.ParsedVersion;
        public Version LatestVersion { get; private set; }
        public QModGame Game { get; set; } = QModGame.None;
        public uint ModId { get; }
        private string ModIdEncoded => WebUtility.UrlEncode(ModId.ToString());

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
            : $"https://mods.vc.api.straitjacket.software/v1/games/{NexusDomainName}/mods/{ModIdEncoded}/{DisplayNameEncoded}.json";

        public VersionState State => CurrentVersion switch
        {
            Version current when LatestVersion is Version latest && current < latest => VersionState.Outdated,
            Version current when LatestVersion is Version latest && current > latest => VersionState.Ahead,
            Version _ when LatestVersion is Version => VersionState.Current,
            _ => VersionState.Unknown
        };

        public IQMod QMod { get; }

        public string Prefix => Assembly == Assembly.GetAssembly(typeof(VersionChecker))
                    ? string.Empty
                    : $"[{DisplayName}] ";

        public async Task UpdateLatestVersionAsync()
        {
            if (!QMod.IsLoaded)
            {
                return;
            }

            try
            {
                if (Game == QModGame.None)
                {
                    LatestVersion = await GetLatestVersionAsync();
                }
                else
                {
                    bool success;
                    (success, LatestVersion) = await TryGetNexusAPILatestVersionAsync();
                    if (success)
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(VersionChecker.ApiKey))
                    {
                        (success, LatestVersion) = await TryGetVersionCheckerAPILatestVersionAsync();
                        if (success)
                        {
                            return;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(URL))
                    {
                        throw new InvalidOperationException("Could not retrieve version from Nexus Mods API and no VersionChecker github URL is defined as a fallback.");
                    }

                    LatestVersion = await GetLatestVersionAsync();
                }
            }
            catch (WebException e)
            {
                _ = Logger.LogErrorAsync($"{Prefix}There was an error retrieving the latest version: " +
                    $"Could not connect to address {URL}");
                _ = Logger.LogErrorAsync(e.Message);
            }
            catch (JsonReaderException e)
            {
                _ = Logger.LogErrorAsync($"{Prefix}There was an error retrieving the latest version: " +
                    $"Invalid JSON found at address {URL}");
                _ = Logger.LogErrorAsync(e.Message);
            }
            catch (JsonSerializationException e)
            {
                _ = Logger.LogErrorAsync($"{Prefix}There was an error retrieving the latest version:");
                _ = Logger.LogErrorAsync(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _ = Logger.LogErrorAsync($"{Prefix}There was an error retrieving the latest version:");
                _ = Logger.LogErrorAsync(e.Message);
            }
            catch (Exception e)
            {
                _ = Logger.LogErrorAsync($"{Prefix}There was an unhandled error retrieving the latest version.");
                _ = Logger.LogErrorAsync(e.ToString());
            }
        }

        public string Message(bool splitLines = false) => State switch
        {
            VersionState.Ahead => $"Currently running v{CurrentVersion}.{(splitLines ? Environment.NewLine : " ")}" +
                                  $"The latest release version is v{LatestVersion}. We are ahead.",
            VersionState.Current => $"Currently running v{CurrentVersion}.{(splitLines ? Environment.NewLine : " ")}" +
                                    $"The latest release version is v{LatestVersion}. Up to date.",
            VersionState.Outdated => $"A new version has been released: v{LatestVersion}.{(splitLines ? Environment.NewLine : " ")}" +
                                     $"Currently running v{CurrentVersion}. Please update at your earliest convenience!",
            _ => "Could not compare versions."
        };

        public VersionRecord(IQMod qMod, string url)
        {
            if (qMod is null)
            {
                throw new ArgumentNullException("qMod");
            }

            QMod = qMod;
            URL = url;

            if (CurrentVersion is null)
            {
                Logger.LogError($"{Prefix}There was an error retrieving the current version: QModManager failed to parse.");
                throw new InvalidOperationException();
            }

            Logger.LogInfo($"{Prefix}Currently running v{CurrentVersion}.");
        }

        public VersionRecord(IQMod qMod, QModGame game, uint modId, string url = null) : this(qMod, url)
        {
            Game = game;
            ModId = modId;
        }

        private async Task<Version> GetLatestVersionAsync()
        {
            ModJson JSON = await Networking.ReadJSONAsync<ModJson>(URL);
            return VersionParser.GetVersion(JSON.Version);
        }

        private async Task<(bool, Version)> TryGetNexusAPILatestVersionAsync()
        {
            try
            {
                return (true, await GetNexusAPILatestVersionAsync());
            }
            catch (WebException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version: " +
                    $"Could not connect to address {URL}");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (JsonReaderException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version: " +
                    $"Invalid JSON found at address {URL}");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (JsonSerializationException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version:");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version:");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (Exception e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an unhandled error retrieving the latest version.");
                _ = Logger.LogWarningAsync(e.ToString());
            }
            return (false, null);
        }

        private async Task<Version> GetNexusAPILatestVersionAsync()
        {
            if (string.IsNullOrWhiteSpace(VersionChecker.ApiKey))
            {
                return await GetVersionCheckerAPILatestVersionAsync();
            }

            string url = NexusAPIModUrl;
            Dictionary<string, string> headers = new Dictionary<string, string> { ["apikey"] = VersionChecker.ApiKey };
            NexusAPI.ModJson JSON = await Networking.ReadJSONAsync<NexusAPI.ModJson>(url, headers);

            if (JSON.Available)
            {
                return VersionParser.GetVersion(JSON.Version);
            }
            else
            {
                // if the mod is unavailable on nexus mods, return the current version so the user is not constantly
                // bugged to update a mod they can't access
                return CurrentVersion;
            }
        }

        private async Task<(bool, Version)> TryGetVersionCheckerAPILatestVersionAsync()
        {
            try
            {
                return (true, await GetVersionCheckerAPILatestVersionAsync());
            }
            catch (WebException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version: " +
                    $"Could not connect to address {URL}");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (JsonReaderException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version: " +
                    $"Invalid JSON found at address {URL}");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (JsonSerializationException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version:");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an error retrieving the latest version:");
                _ = Logger.LogWarningAsync(e.Message);
            }
            catch (Exception e)
            {
                _ = Logger.LogWarningAsync($"{Prefix}There was an unhandled error retrieving the latest version.");
                _ = Logger.LogWarningAsync(e.ToString());
            }
            return (false, null);
        }

        private async Task<Version> GetVersionCheckerAPILatestVersionAsync()
        {
            string url = VersionCheckerAPIModUrl;
            VersionCheckerAPI.ModJson JSON = await Networking.ReadJSONAsync<VersionCheckerAPI.ModJson>(url);
            return VersionParser.GetVersion(JSON.Version);
        }
    }
}
