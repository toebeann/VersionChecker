#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Utility
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
        public Func<Task> UpdateAsync;

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
                string prefix;
                if (Assembly == Assembly.GetAssembly(typeof(VersionChecker)))
                {
                    prefix = string.Empty;
                }
                else
                {
                    prefix = $"[{DisplayName}] ";
                }

                if (!await Networking.CheckConnectionAsync(URL))
                {
                    Logger.LogWarning($"{prefix}Unable to check for updates: Connection unavailable.");
                    return;
                }

                try
                {
                    await UpdateAsync();
                }
                catch (WebException e)
                {
                    Logger.LogError($"{prefix}There was an error retrieving the latest version: " +
                        $"Could not connect to address {URL}");
                    Logger.LogError(e.Message);
                }
                catch (JsonReaderException e)
                {
                    Logger.LogError($"{prefix}There was an error retrieving the latest version: " +
                        $"Invalid JSON found at address {URL}");
                    Logger.LogError(e.Message);
                }
                catch (JsonSerializationException e)
                {
                    Logger.LogError($"{prefix}There was an error retrieving the latest version:");
                    Logger.LogError(e.Message);
                }
                catch (InvalidOperationException e)
                {
                    Logger.LogError($"{prefix}There was an error retrieving the latest version:");
                    Logger.LogError(e.Message);
                }
                catch (Exception e)
                {
                    Logger.LogError($"{prefix}There was an unhandled error retrieving the latest version.");
                    Logger.LogError(e);
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
