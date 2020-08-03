using System;
using System.IO;
using System.Linq;
using Oculus.Newtonsoft.Json;
using QModManager.API;
using QModManager.API.ModLoading;

namespace Straitjacket.Utility
{
    /// <summary>
    /// QModManager entry patcher
    /// </summary>
    [Obsolete("Should not be used!", true)]
    [QModCore]
    public class Patcher
    {
        /// <summary>
        /// QModManager entry point
        /// </summary>
        [Obsolete("Should not be used!", true)]
        [QModPrePatch("468FFFD5F36B7F5D4423044475F0B5F4")]
        public static void Patch()
        {
            VersionChecker.Check(
                "https://github.com/tobeyStraitjacket/VersionChecker/raw/master/VersionChecker/mod.json",
                QModServices.Main.FindModById("VersionChecker")
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
                    Console.WriteLine($"[VersionChecker] Encountered an error while attempting to parse JSON: {modJsonPath}");
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
