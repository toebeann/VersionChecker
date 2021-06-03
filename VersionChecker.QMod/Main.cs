#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#elif BELOWZERO
using Newtonsoft.Json;
#endif
using QModManager.API;
using QModManager.API.ModLoading;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    using ExtensionMethods;
    using NexusIds = Constants.NexusIds;

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
            VersionChecker.Main.Check(new QModJson()
            {
                Version = Constants.Version,
                DisplayName = Constants.QModDisplayName,
                Id = Constants.QModId,
                Enable = true,
                NexusId = new QModJson.NexusIdOptions()
                {
#if SUBNAUTICA
                    Subnautica = NexusIds.VersionChecker.Subnautica.ToString()
#elif BELOWZERO
                    BelowZero = NexusIds.VersionChecker.BelowZero.ToString()
#endif
                },
                VersionChecker = new QModJson.VersionCheckerOptions()
                {
#if SUBNAUTICA
                    LatestVersionURL = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_SUBNAUTICA.json"
#elif BELOWZERO
                    LatestVersionURL = "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod_BELOWZERO.json"
#endif
                }
            });
        }

        private static void InitialiseQModManagerVersionChecks()
        {
            Assembly qmmAssembly = Assembly.GetAssembly(typeof(IQMod));
            AssemblyName qmmAssemblyName = qmmAssembly.GetName();

            VersionChecker.Main.Check(new QModJson()
            {
                Version = qmmAssemblyName.Version.ToStringParsed(),
                DisplayName = "QModManager",
                Id = "QModManager",
                Enable = true,
                NexusId = new QModJson.NexusIdOptions()
                {
#if SUBNAUTICA
                    Subnautica = NexusIds.QModManager.Subnautica.ToString()
#elif BELOWZERO
                    BelowZero = NexusIds.QModManager.BelowZero.ToString()
#endif
                }
            });
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
                        var modJson = JsonConvert.DeserializeObject<QModJson>(File.ReadAllText(modJsonPath));
                        if (!modJson.Enable)
                        {
                            Logger.LogInfo($"Skipping disabled mod: {modJson.DisplayName}");
                            continue;
                        }

                        VersionChecker.Main.Check(modJson);
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
