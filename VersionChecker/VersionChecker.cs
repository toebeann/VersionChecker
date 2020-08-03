using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QModManager.API;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Straitjacket.Utility
{
    /// <summary>
    /// An API for checking the client is running the latest version of a mod, informing the user if it is not.
    /// </summary>
    public partial class VersionChecker : MonoBehaviour
    {
        private static VersionChecker main = null;
        internal static VersionChecker Singleton() => main = main ?? new GameObject("VersionChecker").AddComponent<VersionChecker>();

        private static Dictionary<IQMod, VersionRecord> CheckedVersions = new Dictionary<IQMod, VersionRecord>();

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in plain text at a given URL as an Assembly Version.
        /// </summary>
        /// <param name="URL">The URL at which the plain text file containing the latest version number can be found.</param>
        /// <param name="currentVersion">A <see cref="Version"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod.json or the compiled assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod.json or the mod's
        /// compiled assembly.</param>
        [Obsolete("This method is deprecated in favour of adding the VersionChecker object to your mod.json. Please see the wiki for details.")]
        public static void Check(string URL, Version currentVersion = null, string displayName = null)
        {
            Singleton();


            var assembly = Assembly.GetCallingAssembly();
            var qMod = QModServices.Main.FindModByAssembly(assembly);
            if (qMod == null)
            {
                throw new NullReferenceException();
            }
            if (CheckedVersions.ContainsKey(qMod))
            {
                return;
            }

            if (currentVersion == null)
            {
                currentVersion = qMod.ParsedVersion;
            }
            if (displayName == null)
            {
                displayName = qMod.DisplayName;
            }

            string prefix;
            if (assembly == Assembly.GetAssembly(typeof(VersionChecker)))
            {
                prefix = "[VersionChecker]";
            }
            else
            {
                prefix = $"[VersionChecker] [{displayName}]";
            }

            if (currentVersion == null)
            {

                Console.WriteLine($"{prefix} There was an error retrieving the current version.");
                return;
            }

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = assembly,
                DisplayName = displayName,
                Colour = GetColour(),
                URL = URL,
                CurrentVersion = currentVersion,
                Update = () =>
                {
                    if (TryGetLatestVersion(URL, out var latestVersion))
                    {
                        CheckedVersions[qMod].LatestVersion = latestVersion;
                        return true;
                    }
                    return false;
                }
            };
            Console.WriteLine($"{prefix} Currently running v{currentVersion}.");
        }

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in a JSON file at a given URL as an Assembly Version.
        /// </summary>
        /// <typeparam name="TJsonObject">The type of the class which will be used for deserializing the JSON file.</typeparam>
        /// <param name="URL">The URL at which the JSON file containing the latest version number can be found.</param>
        /// <param name="versionProperty">The name of the property in <typeparamref name="TJsonObject"/> which holds the version number.</param>
        /// <param name="currentVersion">A <see cref="Version"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod.json or the compiled assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod.json or the mod's
        /// compiled assembly.</param>
        [Obsolete("This method is deprecated in favour of adding the VersionChecker object to your mod.json. Please see the wiki for details.")]
        public static void Check<TJsonObject>(string URL, string versionProperty = "Version", Version currentVersion = null, string displayName = null)
            where TJsonObject : class
        {
            Singleton();

            var assembly = Assembly.GetCallingAssembly();
            var qMod = QModServices.Main.FindModByAssembly(assembly);
            if (qMod == null)
            {
                throw new NullReferenceException();
            }
            if (CheckedVersions.ContainsKey(qMod))
            {
                return;
            }

            if (currentVersion == null)
            {
                currentVersion = qMod.ParsedVersion;
            }
            if (displayName == null)
            {
                displayName = qMod.DisplayName;
            }

            string prefix;
            if (assembly == Assembly.GetAssembly(typeof(VersionChecker)))
            {
                prefix = "[VersionChecker]";
            }
            else
            {
                prefix = $"[VersionChecker] [{displayName}]";
            }

            if (currentVersion == null)
            {
                Console.WriteLine($"{prefix} There was an error retrieving the current version.");
                return;
            }

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = assembly,
                DisplayName = displayName,
                Colour = GetColour(),
                URL = URL,
                CurrentVersion = currentVersion,
                Update = () =>
                {
                    if (TryGetLatestVersion<TJsonObject>(URL, typeof(TJsonObject).GetProperty(versionProperty), out var version))
                    {
                        CheckedVersions[qMod].LatestVersion = version;
                        return true;
                    }
                    return false;
                }
            };
            Console.WriteLine($"{prefix} Currently running v{currentVersion}.");
        }

        internal static void Check(string URL, IQMod qMod)
        {
            Singleton();

            if (qMod == null)
            {
                throw new NullReferenceException();
            }
            if (CheckedVersions.ContainsKey(qMod))
            {
                return;
            }

            string prefix;
            if (qMod.LoadedAssembly == Assembly.GetAssembly(typeof(VersionChecker)))
            {
                prefix = "[VersionChecker]";
            }
            else
            {
                prefix = $"[VersionChecker] [{qMod.DisplayName}]";
            }

            if (qMod.ParsedVersion == null)
            {
                Console.WriteLine($"{prefix} There was an error retrieving the current version.");
                return;
            }

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = qMod.LoadedAssembly,
                DisplayName = qMod.DisplayName,
                Colour = GetColour(),
                URL = URL,
                CurrentVersion = qMod.ParsedVersion,
                Update = () =>
                {
                    if (TryGetLatestVersion<ModJson>(URL, typeof(ModJson).GetProperty("Version"), out var version))
                    {
                        CheckedVersions[qMod].LatestVersion = version;
                        return true;
                    }
                    return false;
                }
            };
            Console.WriteLine($"{prefix} Currently running v{qMod.ParsedVersion}.");
        }

        internal static Version GetLatestVersion(string URL)
        {
            if (Networking.TryReadAllText(URL, out var text) && !string.IsNullOrWhiteSpace(text))
            {
                return new Version(text.Trim());
            }

            throw new NullReferenceException();
        }
        internal static bool TryGetLatestVersion(string URL, out Version latestVersion)
        {
            try
            {
                latestVersion = GetLatestVersion(URL);
                return true;
            }
            catch
            {
                latestVersion = null;
                return false;
            }
        }

        internal static Version GetLatestVersion<TJsonObject>(string URL, PropertyInfo versionProperty)
            where TJsonObject : class
        {
            if (versionProperty.PropertyType != typeof(string))
            {
                throw new ArgumentException("A property of Type string is required.", "versionProperty");
            }

            if (Networking.TryReadJSON<TJsonObject>(URL, out var JSON))
            {
                return new Version((string)versionProperty.GetValue(JSON, null));
            }

            throw new NullReferenceException();
        }
        internal static bool TryGetLatestVersion<TJsonObject>(string URL, PropertyInfo versionProperty, out Version latestVersion)
            where TJsonObject : class
        {
            try
            {
                latestVersion = GetLatestVersion<TJsonObject>(URL, versionProperty);
                return true;
            }
            catch
            {
                latestVersion = null;
                return false;
            }
        }

        internal static Config config = new Config();
        private void Awake()
        {
            if (main != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                DontDestroyOnLoad(this);
                config.Load();
                OptionsPanelHandler.RegisterModOptions(new Options());
                SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            }
        }

        private void OnDestroy()
        {
            if (main == this)
            {
                Singleton().StopAllCoroutines();
                main = null;
            }
        }

        private bool IsRunning = false;
        private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Singleton() != null && !Singleton().IsRunning)
            {
                if (scene.name == "StartScreen")
                {
                    Singleton().IsRunning = true;
                    Singleton().StartCoroutine(Singleton().PrintOutdatedVersions());
                    SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
                }
            }
        }

        private static readonly string[] assemblyColours = new string[]
        {
            "aqua", "brown", "green", "lightblue", "teal",
            "lime", "magenta", "olive", "orange", "yellow"
        };
        private static List<string> assignedColours = new List<string>();
        private static string GetColour()
        {
            var availableColours = assemblyColours.Except(assignedColours).Union(assignedColours.Except(assignedColours));
            if (!availableColours.Any())
            {
                var result = assignedColours.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
                while (result.Max(x => x.Value) != result.Min(x => x.Value))
                {
                    result = result.Except(result.Where(x => x.Value == result.Max(y => y.Value)))
                        .ToDictionary(x => x.Key, x => x.Value);
                }
                availableColours = result.Keys.Distinct();
            }

            var colour = availableColours.ElementAt(UnityEngine.Random.Range(0, availableColours.Count() - 1));
            assignedColours.Add(colour);
            return colour;
        }

        private IEnumerator PrintOutdatedVersions()
        {
            yield return new WaitUntil(() => ShouldCheckVersions());
            foreach (var versionRecord in CheckedVersions.Values)
            {
                versionRecord.UpdateLatestVersion();
            }
            config.LastChecked = DateTime.UtcNow;
            config.Save();

            yield return new WaitForSecondsRealtime(1);
            yield return new WaitForFixedUpdate();
            yield return new WaitWhile(() => WaitScreen.main?.isShown ?? false);
            yield return new WaitForFixedUpdate();

            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (var versionRecord in CheckedVersions.Values)
            {
                coroutines.Add(StartCoroutine(PrintOutdatedVersion(versionRecord)));
            }

            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }
        }

        private IEnumerator PrintOutdatedVersion(VersionRecord versionRecord)
        {
            if (versionRecord.State == VersionRecord.VersionState.Outdated)
            {
                for (var i = 0; i < 3; i++)
                {
                    ErrorMessage.AddError($"[<color={versionRecord.Colour}>{versionRecord.DisplayName}</color>] " +
                        versionRecord.Message(true));

                    yield return new WaitForSeconds(5);
                }
            }
        }

        private bool startupChecked = false;
        private bool ShouldCheckVersions()
        {
            if (config.Frequency == CheckFrequency.Startup)
            {
                return startupChecked ? false : startupChecked = true;
            }

            startupChecked = true;

            switch (config.Frequency)
            {
                default:
                case CheckFrequency.Never:
                    return false;
                case CheckFrequency.Hourly:
                    return DateTime.UtcNow > config.LastChecked.AddHours(1);
                case CheckFrequency.Daily:
                    return DateTime.UtcNow > config.LastChecked.AddDays(1);
                case CheckFrequency.Weekly:
                    return DateTime.UtcNow > config.LastChecked.AddDays(7);
                case CheckFrequency.Monthly:
                    return DateTime.UtcNow > config.LastChecked.AddMonths(1);
            }
        }
    }
}
