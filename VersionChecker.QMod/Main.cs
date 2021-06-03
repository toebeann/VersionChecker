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

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
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
            VersionChecker.Main.Check(QModServices.Main.GetMyMod(), QModGame.Subnautica, 467, url);
#elif BELOWZERO
            var url = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_BELOWZERO.json";
            VersionChecker.Main.Check(QModServices.Main.GetMyMod(), QModGame.BelowZero, 66, url);
#endif
        }

        private static void InitialiseQModManagerVersionChecks()
        {
#if SUBNAUTICA
            VersionChecker.Main.Check(new QModManagerQMod(), QModGame.Subnautica, 201);
#elif BELOWZERO
            VersionChecker.Main.Check(new QModManagerQMod(), QModGame.BelowZero, 1);
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
                        {
                            Logger.LogInfo($"Skipping disabled mod: {modJson.DisplayName}");
                            continue;
                        }


                        IQMod qMod = QModServices.Main.FindModById(modJson.Id);
                        if (qMod is null)
                        {
                            Logger.LogWarning($"Skipping mod due to QModServices error: {modJson.DisplayName}.");
                            continue;
                        }

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
                                int modId = int.Parse(modIdString, CultureInfo.InvariantCulture.NumberFormat);

                                VersionChecker.Main.Check(qMod, game, modId, modJson.VersionChecker?.LatestVersionURL);
                                break;
                            }
                            catch (ArgumentNullException e)
                            {
                                Logger.LogError($"[{modJson.DisplayName}] Skipping NexusId: mod ID is null: {modIdString}");
                                Logger.LogError(e.Message);
                            }
                            catch (FormatException e)
                            {
                                Logger.LogError($"[{modJson.DisplayName}] Skipping NexusId: mod ID is not a valid unsigned integer: {modIdString}");
                                Logger.LogError(e.Message);
                            }
                            catch (OverflowException e)
                            {
                                Logger.LogError($"[{modJson.DisplayName}] Skipping NexusId: mod ID is outside the valid range for an unsigned integer: " +
                                    $"{modIdString}");
                                Logger.LogError(e.Message);
                            }
                        }

                        if (modJson.VersionChecker != null)
                        {
                            VersionChecker.Main.Check(qMod, modJson.VersionChecker.LatestVersionURL);
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
