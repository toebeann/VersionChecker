using System;

namespace Straitjacket.Subnautica.Mods.VersionChecker
{
    internal interface IConfig
    {
        public CheckFrequency Frequency { get; }
        public DateTime LastChecked { get; }
    }
}
