using System;

namespace Straitjacket.Utility.VersionFormats
{
    /// <summary>
    /// Represents a version number formatted under Assembly Versioning.
    /// </summary>
    public class AssemblyVersion : VersionFormat
    {
        /// <summary>
        /// Initiailises a new <see cref="AssemblyVersion"/> instance.
        /// </summary>
        /// <seealso cref="AssemblyVersion(string)"/>
        /// <seealso cref="AssemblyVersion(Version)"/>
        public AssemblyVersion() { }

        /// <summary>
        /// Initailises a new <see cref="AssemblyVersion"/> instance using the specified <see cref="string"/>.
        /// </summary>
        /// <param name="version">A string representing an Assembly Versioning version number.</param>
        /// <seealso cref="AssemblyVersion(Version)"/>
        /// <seealso cref="AssemblyVersion()"/>
        public AssemblyVersion(string version) => Version = new Version(version).ToStringParsed();

        /// <summary>
        /// Initialises a new <see cref="AssemblyVersion"/> instance using the specified <see cref="AssemblyVersion"/>.
        /// </summary>
        /// <param name="version">A <see cref="Version"/> representing an Assembly Versioning version number.</param>
        /// <seealso cref="AssemblyVersion(string)"/>
        /// <seealso cref="AssemblyVersion()"/>
        public AssemblyVersion(Version version) => Version = version.ToStringParsed();

        /// <summary>
        /// Compares the current <see cref="AssemblyVersion"/> to a specified <see cref="VersionFormat"/> and returns an indicataion of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="VersionFormat"/> representing an Assembly Versioning version number.</param>
        /// <returns>An <see cref="int"/> indicating the relative values of the two Assembly Versioning version numbers.</returns>
        public override int CompareTo(VersionFormat other) => new Version(Version).CompareTo(new Version(other.Version));
    }
}
