using QModManager.API.ModLoading;

namespace Straitjacket.Utility
{
    [QModCore]
    public class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches() {
            VersionChecker.Check<ModJson>(
                "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod.json"
            );
        }
    }
}
