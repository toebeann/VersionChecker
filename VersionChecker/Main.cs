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
using System.Globalization;
using System.IO;
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
        [QModPatch]
        public static void Patch()
        {
            Logger.LogInfo("Initialising...");
            var stopwatch = Stopwatch.StartNew();

            InitializeVersionCheckerVersionChecks();
            InitialiseQModManagerVersionChecks();
            InitialiseQModVersionChecks();

            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static void InitializeVersionCheckerVersionChecks()
        {
#if SUBNAUTICA
            var url = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_SUBNAUTICA.json";
            VersionChecker.Check(QModGame.Subnautica, 467, QModServices.Main.GetMyMod(), url);
#elif BELOWZERO
            var url = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_BELOWZERO.json";
            VersionChecker.Check(QModGame.BelowZero, 66, QModServices.Main.GetMyMod(), url);
#endif
        }

        private static void InitialiseQModManagerVersionChecks()
        {
#if SUBNAUTICA
            VersionChecker.Check(QModGame.Subnautica, 201, new QModManagerQMod());
#elif BELOWZERO
            VersionChecker.Check(QModGame.BelowZero, 1, new QModManagerQMod());
#endif
        }

        private static void InitialiseQModVersionChecks()
        {
            var QModsPath = Path.Combine(Environment.CurrentDirectory, "QMods");
            var subfolders = Directory.GetDirectories(QModsPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var subfolder in subfolders)
            {
                if (subfolder.EndsWith($"{Path.DirectorySeparatorChar}.backups"))
                    continue;

                foreach (var modJsonPath in Directory.GetFiles(subfolder, "mod.json", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var modJson = JsonConvert.DeserializeObject<ModJson>(File.ReadAllText(modJsonPath));
                        if (!modJson.Enable)
                            continue;

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

                            try
                            {
                                uint modId = uint.Parse(modIdString, CultureInfo.InvariantCulture.NumberFormat);

                                VersionChecker.Check(game, modId, qMod, modJson.VersionChecker?.LatestVersionURL);
                                break;
                            }
                            catch (ArgumentNullException e)
                            {
                                Logger.LogError($"Skipping NexusId: mod ID is null: {modIdString}");
                                Logger.LogError(e.Message);
                            }
                            catch (FormatException e)
                            {
                                Logger.LogError($"Skipping NexusId: mod ID is not a valid unsigned integer: {modIdString}");
                                Logger.LogError(e.Message);
                            }
                            catch (OverflowException e)
                            {
                                Logger.LogError($"Skipping NexusId: mod ID is outside the valid range for an unsigned integer: " +
                                    $"{modIdString}");
                                Logger.LogError(e.Message);
                            }
                        }

                        if (modJson.VersionChecker != null)
                        {
                            VersionChecker.Check(modJson.VersionChecker.LatestVersionURL, qMod);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Encountered an error while attempting to parse JSON: {modJsonPath}");
                        Logger.LogError(e);
                    }
                }
            }
        }
    }
}
