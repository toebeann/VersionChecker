using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QModManager.API;

namespace Straitjacket.Utility
{
    internal class QModManagerQMod : IQMod
    {
        public string Id { get; } = "QModManager";

        public string DisplayName { get; } = "QModManager";

        public string Author => throw new NotImplementedException();

        public QModGame SupportedGame { get; }
#if SUBNAUTICA
            = QModGame.Subnautica;
#elif BELOWZERO
            = QModGame.BelowZero;
#endif

        public IEnumerable<RequiredQMod> RequiredMods => throw new NotImplementedException();

        public IEnumerable<string> ModsToLoadBefore => throw new NotImplementedException();

        public IEnumerable<string> ModsToLoadAfter => throw new NotImplementedException();

        public Assembly LoadedAssembly { get; } = Assembly.GetAssembly(typeof(IQMod));

        public string AssemblyName => throw new NotImplementedException();

        public Version ParsedVersion => LoadedAssembly.GetName().Version;

        public bool Enable { get; } = true;

        public bool IsLoaded { get; } = true;
    }
}
