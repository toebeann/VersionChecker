using Straitjacket.Utility.VersionFormats;

namespace Straitjacket.Utility
{
    internal class VersionRecord
    {
        public enum VersionState { Unknown, Outdated, Current, Ahead }

        public string DisplayName;
        public string Colour;
        public string URL;
        public VersionFormat CurrentVersion;
        public VersionFormat LatestVersion;
        public delegate bool TryGetLatestVersionFunc(string URL, out VersionFormat latestVersion);
        public TryGetLatestVersionFunc TryGetLatestVersion;
        public VersionState State
        {
            get
            {
                if (CurrentVersion == null || LatestVersion == null || CurrentVersion.Version == null || LatestVersion.Version == null)
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

        public void Update()
        {
            if (TryGetLatestVersion != null && TryGetLatestVersion(URL, out var latestVersion))
            {
                LatestVersion = latestVersion;
            }
        }
    }
}
