using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System;

namespace Straitjacket.Utility
{
    [Menu("VersionChecker", LoadOn = MenuAttribute.LoadEvents.MenuOpened | MenuAttribute.LoadEvents.MenuRegistered)]
    internal class Config : ConfigFile
    {
        [Choice("Frequency of checks")]
        public VersionChecker.CheckFrequency Frequency = VersionChecker.CheckFrequency.Hourly;

        public DateTime LastChecked { get; set; }
    }
}
