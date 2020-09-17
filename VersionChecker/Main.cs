using Oculus.Newtonsoft.Json;
using QModManager.API;
using QModManager.API.ModLoading;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Utility
{
    /// <summary>
    /// QModManager entry patcher
    /// </summary>
    [Obsolete("Should not be used!", true)]
    [QModCore]
    public class Main
    {
        /// <summary>
        /// QModManager entry point
        /// </summary>
        [Obsolete("Should not be used!", true)]
        [QModPrePatch("468FFFD5F36B7F5D4423044475F0B5F4")]
        public static void Patch()
        {
            Logger.LogInfo("Initialising...");
            var stopwatch = Stopwatch.StartNew();

            VersionChecker.Check(
                "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod.json",
                QModServices.Main.GetMyMod()
            );

            var QModsPath = Path.Combine(Environment.CurrentDirectory, "QMods");
            foreach (var modJsonPath in Directory.GetDirectories(QModsPath, "*", SearchOption.TopDirectoryOnly)
                .SelectMany(subfolder => Directory.GetFiles(subfolder, "mod.json", SearchOption.TopDirectoryOnly)))
            {
                try
                {
                    var modJson = JsonConvert.DeserializeObject<ModJson>(File.ReadAllText(modJsonPath));
                    if (modJson.VersionChecker != null)
                    {
                        VersionChecker.Check(modJson.VersionChecker.LatestVersionURL, QModServices.Main.FindModById(modJson.Id));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Encountered an error while attempting to parse JSON: {modJsonPath}");
                    Logger.LogError(ex.Message);
                }
            }

            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}
