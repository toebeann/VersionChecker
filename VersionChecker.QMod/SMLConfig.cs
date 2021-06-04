using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System;

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    [Menu("VersionChecker", LoadOn = MenuAttribute.LoadEvents.MenuOpened | MenuAttribute.LoadEvents.MenuRegistered)]
    internal class SMLConfig : ConfigFile, ISMLConfig
    {
        [Choice("Frequency of checks")]
        public CheckFrequency Frequency { get; set; } = CheckFrequency.Hourly;

        public DateTime LastChecked { get; set; }
    }
}
