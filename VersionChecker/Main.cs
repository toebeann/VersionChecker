#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using BepInEx;
using QModManager.API;
using QModManager.API.ModLoading;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Utility.VersionChecker
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

#if SUBNAUTICA
            var url = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_SUBNAUTICA.json";
            VersionChecker.Check(QModGame.Subnautica, 467, QModServices.Main.GetMyMod(), url);
#elif BELOWZERO
            var url = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_BELOWZERO.json";
            VersionChecker.Check(QModGame.BelowZero, 66, QModServices.Main.GetMyMod(), url);
#endif

            var QModsPath = Path.Combine(Environment.CurrentDirectory, "QMods");
            foreach (var modJsonPath in Directory.GetDirectories(QModsPath, "*", SearchOption.TopDirectoryOnly)
                .SelectMany(subfolder => Directory.GetFiles(subfolder, "mod.json", SearchOption.TopDirectoryOnly)))
            {
                try
                {
                    var modJson = JsonConvert.DeserializeObject<ModJson>(File.ReadAllText(modJsonPath));
                    IQMod qMod = QModServices.Main.FindModById(modJson.Id);

                    if (modJson.NexusId != null && (modJson.NexusId.Subnautica != null || modJson.NexusId.BelowZero != null))
                    {
                        QModGame game = Paths.ProcessName switch
                        {
                            "Subnautica" when modJson.NexusId.Subnautica != null => QModGame.Subnautica,
                            "Subnautica" when modJson.NexusId.BelowZero != null => QModGame.BelowZero,
                            "SubnauticaZero" when modJson.NexusId.BelowZero != null => QModGame.BelowZero,
                            "SubnauticaZero" when modJson.NexusId.Subnautica != null => QModGame.Subnautica,
                            _ => QModGame.None
                        };

                        string modIdString = game switch
                        {
                            QModGame.Subnautica => modJson.NexusId.Subnautica,
                            QModGame.BelowZero => modJson.NexusId.BelowZero,
                            _ => null
                        };

                        int modId = int.Parse(modIdString);

                        VersionChecker.Check(game, modId, qMod, modJson.VersionChecker?.LatestVersionURL);
                        break;
                    }

                    if (modJson.VersionChecker != null)
                    {
                        VersionChecker.Check(modJson.VersionChecker.LatestVersionURL, qMod);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Encountered an error while attempting to parse JSON: {modJsonPath}");
                    Logger.LogError(ex);
                }
            }

            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}
