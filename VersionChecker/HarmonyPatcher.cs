using System;
using QModManager.API.ModLoading;

namespace Straitjacket.Utility
{
    /// <summary>
    /// QModManager entry patcher
    /// </summary>
    [Obsolete("Should not be used!", true)]
    [QModCore]
    public class HarmonyPatcher
    {
        /// <summary>
        /// QModManager entry point
        /// </summary>
        [Obsolete("Should not be used!", true)]
        [QModPrePatch]
        public static void ApplyPatches()
        {
            VersionChecker.Check<ModJson>(
                "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod.json"
            );
        }
    }
}
