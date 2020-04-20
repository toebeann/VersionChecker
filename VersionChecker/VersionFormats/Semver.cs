using Semver;

namespace Straitjacket.Utility.VersionFormats
{
    /// <summary>
    /// Represents a version number formatted under Semantic Versioning (SemVer).
    /// </summary>
    public class SemVer : VersionFormat
    {
        /// <summary>
        /// Initialises a new <see cref="SemVer"/> instance.
        /// </summary>
        /// <seealso cref="SemVer(string)"/>
        /// <seealso cref="SemVer(SemVersion)"/>
        public SemVer() { }

        /// <summary>
        /// Initialises a new <see cref="SemVer"/> instance using the specified <see cref="string"/>.
        /// </summary>
        /// <param name="version">A string representing a SemVer version number.</param>
        /// <seealso cref="SemVer(SemVersion)"/>
        /// <seealso cref="SemVer()"/>
        public SemVer(string version) => Version = SemVersion.Parse(version).ToString();

        /// <summary>
        /// Initialises a new <see cref="SemVer"/> instance using the specified <see cref="SemVersion"/>.
        /// </summary>
        /// <param name="version">A <see cref="SemVersion"/> representing a SemVer version number.</param>
        /// <seealso cref="SemVer(string)"/>
        /// <seealso cref="SemVer()"/>
        public SemVer(SemVersion version) => Version = version.ToString();

        /// <summary>
        /// Compares the current <see cref="SemVer"/> to a specified <see cref="VersionFormat"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="VersionFormat"/> representing a SemVer version number.</param>
        /// <returns>An <see cref="int"/> indicating the relative values of the two SemVer version numbers.</returns>
        public override int CompareTo(VersionFormat other) => SemVersion.Parse(Version).CompareTo(SemVersion.Parse(other.Version));
    }
}
