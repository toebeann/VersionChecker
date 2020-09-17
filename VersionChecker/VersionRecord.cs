using System;
using System.Reflection;
using UnityEngine;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Utility
{
    public partial class VersionChecker : MonoBehaviour
    {
        internal class VersionRecord
        {
            public enum VersionState { Unknown, Outdated, Current, Ahead }

            public Assembly Assembly;
            public string DisplayName;
            public string Colour;
            public string URL;
            public Version CurrentVersion;
            public Version LatestVersion;
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

            public void UpdateLatestVersion()
            {
                if (Update != null)
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

                    if (!Networking.CheckConnection(URL))
                    {
                        Logger.LogWarning($"{prefix}Unable to check for updates: Connection unavailable.");
                        return;
                    }

                    if (!Update())
                    {
                        Logger.LogError($"{prefix}There was an error retrieving the latest version.");
                        return;
                    }
                }
            }
            public Func<bool> Update;

            public string Message(bool splitLines = false)
            {
                switch (State)
                {
                    case VersionState.Ahead:
                        return $"Currently running v{CurrentVersion}." +
                        (splitLines ? Environment.NewLine : " ") +
                        $"The latest release version is v{LatestVersion}. " +
                        $"We are ahead.";
                    case VersionState.Current:
                        return $"Currently running v{CurrentVersion}. Up to date.";
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
}
