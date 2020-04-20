using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Straitjacket.Utility.VersionFormats;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Straitjacket.Utility
{
    /// <summary>
    /// An API for checking the client is running the latest version of a mod, informing the user if it is not.
    /// </summary>
    public class VersionChecker : MonoBehaviour
    {
        private static VersionChecker main = null;
        internal static VersionChecker Singleton() => main = main ?? new GameObject("VersionChecker").AddComponent<VersionChecker>();

        private static Dictionary<Assembly, VersionRecord> CheckedVersions = new Dictionary<Assembly, VersionRecord>();

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in plain text at a given URL as an Assembly Version.
        /// </summary>
        /// <param name="URL">The URL at which the plain text file containing the latest version number can be found.</param>
        /// <param name="currentVersion">A <see cref="Version"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod's assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod's assembly.</param>
        public static void Check(string URL, Version currentVersion = null, string displayName = null)
        {
            Singleton();

            var assembly = Assembly.GetCallingAssembly();
            if (CheckedVersions.ContainsKey(assembly))
            {
                return;
            }

            currentVersion = currentVersion ?? assembly.GetName().Version;
            displayName = displayName ?? assembly.GetName().Name;

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

            if (!Networking.CheckConnection(URL))
            {
                Console.WriteLine($"{prefix} Unable to check for updates: Connection unavailable.");
                return;
            }

            if (!TryGetLatestVersion<AssemblyVersion>(URL, out var latestVersion))
            {
                Console.WriteLine($"{prefix} There was an error retrieving the latest version.");
                return;
            }

            var versionRecord = CheckedVersions[assembly] = new VersionRecord
            {
                DisplayName = displayName,
                Colour = GetColour(),
                CurrentVersion = new AssemblyVersion(currentVersion),
                LatestVersion = latestVersion
            };

            Console.WriteLine($"{prefix} {VersionMessage(versionRecord)}");
        }

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in plain text at a given URL, using the specified
        /// <typeparamref name="TVersionFormat"/> as the formatting for the version number.
        /// </summary>
        /// <typeparam name="TVersionFormat">The formatting to use for version number parsing and comparisons.</typeparam>
        /// <param name="URL">The URL at which the plain text file containing the latest version number can be found.</param>
        /// <param name="currentVersion">A <typeparamref name="TVersionFormat"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod's assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod's assembly.</param>
        public static void Check<TVersionFormat>(string URL, TVersionFormat currentVersion = null, string displayName = null)
            where TVersionFormat : VersionFormat, new()
        {
            Singleton();

            var assembly = Assembly.GetCallingAssembly();
            if (CheckedVersions.ContainsKey(assembly))
            {
                return;
            }

            if (currentVersion == null)
            {
                currentVersion = new TVersionFormat { Version = assembly.GetName().Version.ToStringParsed() };
            }
            displayName = displayName ?? assembly.GetName().Name;

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

            if (!Networking.CheckConnection(URL))
            {
                Console.WriteLine($"{prefix} Unable to check for updates: Connection unavailable.");
                return;
            }

            if (!TryGetLatestVersion<TVersionFormat>(URL, out var latestVersion))
            {
                Console.WriteLine($"{prefix} There was an error retrieving the latest version.");
                return;
            }

            var versionRecord = CheckedVersions[assembly] = new VersionRecord
            {
                DisplayName = displayName,
                Colour = GetColour(),
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion
            };

            Console.WriteLine($"{prefix} {VersionMessage(versionRecord)}");
        }

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in a JSON file at a given URL as an Assembly Version.
        /// </summary>
        /// <typeparam name="TJsonObject">The type of the class which will be used for deserializing the JSON file.</typeparam>
        /// <param name="URL">The URL at which the JSON file containing the latest version number can be found.</param>
        /// <param name="versionProperty">The name of the property in <typeparamref name="TJsonObject"/> which holds the version number.</param>
        /// <param name="currentVersion">A <see cref="Version"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod's assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod's assembly.</param>
        public static void Check<TJsonObject>(string URL, string versionProperty = "Version", Version currentVersion = null, string displayName = null)
            where TJsonObject : class
        {
            Singleton();

            var assembly = Assembly.GetCallingAssembly();
            if (CheckedVersions.ContainsKey(assembly))
            {
                return;
            }

            currentVersion = currentVersion ?? assembly.GetName().Version;
            displayName = displayName ?? assembly.GetName().Name;

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

            if (!Networking.CheckConnection(URL))
            {
                Console.WriteLine($"{prefix} Unable to check for updates: Connection unavailable.");
                return;
            }

            if (!TryGetLatestVersion<AssemblyVersion, TJsonObject>(URL, typeof(TJsonObject).GetProperty(versionProperty), out var latestVersion))
            {
                Console.WriteLine($"{prefix} There was an error retrieving the latest version.");
                return;
            }

            var versionRecord = CheckedVersions[assembly] = new VersionRecord
            {
                DisplayName = displayName,
                Colour = GetColour(),
                CurrentVersion = new AssemblyVersion(currentVersion),
                LatestVersion = latestVersion
            };

            Console.WriteLine($"{prefix} {VersionMessage(versionRecord)}");
        }

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in a JSON file at a given URL, using the specified
        /// <typeparamref name="TVersionFormat"/> as the formatting for the version number.
        /// </summary>
        /// <typeparam name="TVersionFormat">The formatting to use for version number parsing and comparisons.</typeparam>
        /// <typeparam name="TJsonObject">The type of the class which will be used for deserializing the JSON file.</typeparam>
        /// <param name="URL">The URL at which the JSON file containing the latest version number can be found.</param>
        /// <param name="versionProperty">The name of the property in <typeparamref name="TJsonObject"/> which holds the version number.</param>
        /// <param name="currentVersion">A <typeparamref name="TVersionFormat"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod's assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod's assembly.</param>
        public static void Check<TVersionFormat, TJsonObject>(string URL, string versionProperty = "Version",
            TVersionFormat currentVersion = null, string displayName = null)
            where TVersionFormat : VersionFormat, new()
            where TJsonObject : class
        {
            Singleton();

            var assembly = Assembly.GetCallingAssembly();
            if (CheckedVersions.ContainsKey(assembly))
            {
                return;
            }

            if (currentVersion == null)
            {
                currentVersion = new TVersionFormat { Version = assembly.GetName().Version.ToStringParsed() };
            }
            displayName = displayName ?? assembly.GetName().Name;

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

            if (!Networking.CheckConnection(URL))
            {
                Console.WriteLine($"{prefix} Unable to check for updates: Connection unavailable.");
                return;
            }

            if (!TryGetLatestVersion<TVersionFormat, TJsonObject>(URL, typeof(TJsonObject).GetProperty(versionProperty), out var latestVersion))
            {
                Console.WriteLine($"{prefix} There was an error retrieving the latest version.");
                return;
            }

            var versionRecord = CheckedVersions[assembly] = new VersionRecord
            {
                DisplayName = displayName,
                Colour = GetColour(),
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion
            };

            Console.WriteLine($"{prefix} {VersionMessage(versionRecord)}");
        }

        private static TVersionFormat GetLatestVersion<TVersionFormat>(string URL)
            where TVersionFormat : VersionFormat, new()
        {
            if (Networking.TryReadAllText(URL, out var text))
            {
                return new TVersionFormat() { Version = text.Trim() };
            }
            return null;
        }
        private static bool TryGetLatestVersion<TVersionFormat>(string URL, out TVersionFormat latestVersion)
            where TVersionFormat : VersionFormat, new()
        {
            try
            {
                latestVersion = GetLatestVersion<TVersionFormat>(URL);
                return true;
            }
            catch
            {
                latestVersion = null;
                return false;
            }
        }

        private static TVersionFormat GetLatestVersion<TVersionFormat, TJsonObject>(string URL, PropertyInfo versionProperty)
            where TVersionFormat : VersionFormat, new()
            where TJsonObject : class
        {
            if (versionProperty.PropertyType != typeof(string))
            {
                throw new ArgumentException("A property of Type string is required.", "versionProperty");
            }

            if (Networking.TryReadJSON<TJsonObject>(URL, out var JSON))
            {
                return new TVersionFormat() { Version = (string)versionProperty.GetValue(JSON, null) };
            }
            return null;
        }
        private static bool TryGetLatestVersion<TVersionFormat, TJsonObject>(string URL, PropertyInfo versionProperty, out TVersionFormat latestVersion)
            where TVersionFormat : VersionFormat, new()
            where TJsonObject : class
        {
            try
            {
                latestVersion = GetLatestVersion<TVersionFormat, TJsonObject>(URL, versionProperty);
                return true;
            }
            catch
            {
                latestVersion = null;
                return false;
            }
        }

        private void Awake()
        {
            if (main != null)
            {
                DestroyImmediate(this);
            }
            else
            {
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
                if (scene.name == "Main")
                {
                    Singleton().IsRunning = true;
                    Singleton().StartCoroutine(Singleton().PrintOutdatedVersions());
                }
                else if (mode == LoadSceneMode.Single)
                {
                    Singleton().IsRunning = false;
                    Singleton().StopAllCoroutines();
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
            yield return new WaitForFixedUpdate();
            yield return new WaitWhile(() => WaitScreen.main == null);
            yield return new WaitWhile(() => WaitScreen.main.isShown);
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

            yield return new WaitForSecondsRealtime(1800);
            yield return PrintOutdatedVersions();
        }

        private IEnumerator PrintOutdatedVersion(VersionRecord versionRecord)
        {
            if (versionRecord.State == VersionRecord.VersionState.Outdated)
            {
                for (var i = 0; i < 3; i++)
                {
                    ErrorMessage.AddError($"[<color={versionRecord.Colour}>{versionRecord.DisplayName}</color>] " +
                        VersionMessage(versionRecord, true));

                    yield return new WaitForSeconds(5);
                }
            }
        }

        private static string VersionMessage(VersionRecord versionRecord, bool splitLines = false)
        {
            switch (versionRecord.State)
            {
                case VersionRecord.VersionState.Ahead:
                    return $"Currently running v{versionRecord.CurrentVersion}." +
                    (splitLines ? Environment.NewLine : " ") +
                    $"The latest release version is v{versionRecord.LatestVersion}. " +
                    $"We are ahead.";
                case VersionRecord.VersionState.Current:
                    return $"Currently running v{versionRecord.CurrentVersion}. Up to date.";
                case VersionRecord.VersionState.Outdated:
                    return $"A new version has been released: v{versionRecord.LatestVersion}." +
                    (splitLines ? Environment.NewLine : " ") +
                    $"Currently running v{versionRecord.CurrentVersion}. " +
                    "Please update at your earliest convenience!";
                case VersionRecord.VersionState.Unknown:
                default:
                    return "Could not compare versions.";
            }
        }
    }
}
