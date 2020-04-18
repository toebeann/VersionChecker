namespace Straitjacket.Utility
{
    internal class HarmonyPatcher
    {
        public static void ApplyPatches() {
            VersionChecker.Check<ModJson>(
                "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod.json"
            );
        }
    }
}
