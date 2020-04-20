namespace Straitjacket.Utility
{
    /// <summary>
    /// A bare-bones definition of a mod.json file for retrieving the Version property.
    /// </summary>
    public class ModJson
    {
        /// <summary>
        /// The Version property containing the version number.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The DisplayName property containing the display name of the mod.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
