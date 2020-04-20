namespace Straitjacket.Utility.VersionFormats
{
    /// <summary>
    /// A simple interface for specifiying version number formats. It is not recommended to implement this interface directly, but instead to
    /// derive from <see cref="VersionFormat"/>.
    /// </summary>
    public interface IVersionFormat
    {
        /// <summary>
        /// A <see cref="string"/> representing the version number of this instance.
        /// </summary>
        string Version { get; set; }
    }
}
