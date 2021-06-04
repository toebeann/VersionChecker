using System;

namespace Straitjacket.Subnautica.Mods.VersionChecker
{
    internal interface ISMLConfig
    {
        public CheckFrequency Frequency { get; }
        public DateTime LastChecked { get; }
    }
}
