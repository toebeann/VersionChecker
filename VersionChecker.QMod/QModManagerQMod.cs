using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QModManager.API;

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    internal class QModManagerQMod : IQMod
    {
        public string Id { get; } = "QModManager";

        public string DisplayName { get; } = "QModManager";

        public string Author { get; } = "QModManager";

        public QModGame SupportedGame { get; }
#if SUBNAUTICA
            = QModGame.Subnautica;
#elif BELOWZERO
            = QModGame.BelowZero;
#endif

        public IEnumerable<RequiredQMod> RequiredMods { get; } = new RequiredQMod[0];

        public IEnumerable<string> ModsToLoadBefore { get; } = new string[0];

        public IEnumerable<string> ModsToLoadAfter { get; } = new string[0];

        public Assembly LoadedAssembly { get; } = Assembly.GetAssembly(typeof(IQMod));

        public string AssemblyName => LoadedAssembly.GetName().Name;

        public Version ParsedVersion => LoadedAssembly.GetName().Version;

        public bool Enable { get; } = true;

        public bool IsLoaded { get; } = true;
    }
}
