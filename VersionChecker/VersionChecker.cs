using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Straitjacket.Utility
{
    public class VersionChecker : MonoBehaviour
    {
        private struct VersionRecord
        {
            public string DisplayName;
            public Version CurrentVersion;
            public Version LatestVersion;
            public string Colour;
        }

        private static VersionChecker main = null;
        public static VersionChecker Main => main = main ?? new GameObject("VersionChecker").AddComponent<VersionChecker>();

        private static Dictionary<Assembly, VersionRecord> CheckedVersions = new Dictionary<Assembly, VersionRecord>();

        public void Check(string URL, Version currentVersion = null, string displayName = null)
        {
            var assembly = Assembly.GetCallingAssembly();
            if (CheckedVersions.ContainsKey(assembly))
            {
                return;
            }

            currentVersion = currentVersion ?? assembly.GetName().Version;
            displayName = displayName ?? assembly.GetName().Name;

            if (currentVersion == null)
            {
                Console.WriteLine($"[VersionChecker] [{displayName}] There was an error retrieving the current version.");
            }

            if (!CheckConnection(URL))
            {
                Console.WriteLine($"[VersionChecker] [{displayName}] Unable to check for updates: Connection unavailable.");
                return;
            }

            var latestVersion = GetLatestVersion(URL);
            if (latestVersion == null)
            {
                Console.WriteLine($"[VersionChecker] [{displayName}] There was an error retrieving the latest version.");
                return;
            }

            var versionRecord = CheckedVersions[assembly] = new VersionRecord
            {
                DisplayName = displayName,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                Colour = GetColour()
            };

            Console.WriteLine($"[VersionChecker] [{displayName}] {VersionMessage(versionRecord)}");
        }
        public void Check<T>(string URL, PropertyInfo versionProperty, Version currentVersion = null, string displayName = null)
            where T : class
        {
            var assembly = Assembly.GetCallingAssembly();
            if (CheckedVersions.ContainsKey(assembly))
            {
                return;
            }

            currentVersion = currentVersion ?? assembly.GetName().Version;
            displayName = displayName ?? assembly.GetName().Name;

            if (currentVersion == null)
            {
                Console.WriteLine($"[VersionChecker] [{displayName}] There was an error retrieving the current version.");
            }

            if (!CheckConnection(URL))
            {
                Console.WriteLine($"[VersionChecker] [{displayName}] Unable to check for updates: Connection unavailable.");
                return;
            }

            var latestVersion = GetLatestVersion<T>(URL, versionProperty);
            if (latestVersion == null)
            {
                Console.WriteLine($"[VersionChecker] [{displayName}] There was an error retrieving the latest version.");
                return;
            }

            var versionRecord = CheckedVersions[assembly] = new VersionRecord
            {
                DisplayName = displayName,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                Colour = GetColour()
            };

            Console.WriteLine($"[VersionChecker] [{displayName}] {VersionMessage(versionRecord)}");
        }
        public void Check<T>(string URL, string versionProperty, Version currentVersion = null, string displayName = null)
            where T : class
        {
            var assembly = Assembly.GetCallingAssembly();
            currentVersion = currentVersion ?? assembly.GetName().Version;
            displayName = displayName ?? assembly.GetName().Name;
            Check<T>(URL, typeof(T).GetProperty(versionProperty), currentVersion, displayName);
        }

        private static Version GetLatestVersion(string URL)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var version = client.DownloadString(URL);
                    return new Version(version.Trim());
                }
            }
            catch
            {
                return null;
            }
        }
        private static Version GetLatestVersion<T>(string URL, PropertyInfo versionProperty) where T : class
        {
            if (versionProperty.PropertyType != typeof(string))
            {
                return null;
            }

            try
            {
                using (var client = new WebClient())
                {
                    var data = client.DownloadString(URL);
                    var deserializedData = JsonConvert.DeserializeObject<T>(data);
                    return new Version((string)versionProperty.GetValue(deserializedData, null));
                }
            }
            catch
            {
                return null;
            }
        }

        private const string GOOGLE_204_URL = "http://google.com/generate_204";
        private static bool CheckConnection(string URL = GOOGLE_204_URL)
        {
            if (URL != GOOGLE_204_URL)
            {
                var preliminary = CheckConnection(GOOGLE_204_URL);
                if (preliminary)
                {
                    return preliminary;
                }
                return CheckConnection(URL);
            }
            else
            {
                try
                {
                    using (var client = new WebClient())
                    using (client.OpenRead(URL))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
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
                Main.StopAllCoroutines();
                main = null;
            }
        }

        private bool IsRunning = false;
        private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Main != null && !Main.IsRunning)
            {
                if (scene.name == "Main")
                {
                    Main.IsRunning = true;
                    Main.StartCoroutine(Main.PrintOutdatedVersions());
                }
                else if (mode == LoadSceneMode.Single)
                {
                    Main.IsRunning = false;
                    Main.StopAllCoroutines();
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
            if (IsOutdated(versionRecord))
            {
                for (var i = 0; i < 3; i++)
                {
                    ErrorMessage.AddError($"[<color={versionRecord.Colour}>{versionRecord.DisplayName}</color>] " +
                        VersionMessage(versionRecord, true));

                    yield return new WaitForSeconds(5);
                }
            }
        }

        private bool IsOutdated(VersionRecord versionRecord) => versionRecord.CurrentVersion < versionRecord.LatestVersion &&
            versionRecord.CurrentVersion.ToStringParsed() != versionRecord.LatestVersion.ToStringParsed();

        private bool IsAhead(VersionRecord versionRecord) => versionRecord.CurrentVersion > versionRecord.LatestVersion &&
            versionRecord.CurrentVersion.ToStringParsed() != versionRecord.LatestVersion.ToStringParsed();

        private string VersionMessage(VersionRecord versionRecord, bool splitLines = false)
        {
            if (IsOutdated(versionRecord))
            {
                return $"A new version has been released: v{versionRecord.LatestVersion.ToStringParsed()}." +
                    (splitLines ? Environment.NewLine : " ") +
                    $"Currently running v{versionRecord.CurrentVersion.ToString()}. " +
                    "Please update at your earliest convenience!";
            }
            else if (IsAhead(versionRecord))
            {
                return $"Currently running v{versionRecord.CurrentVersion.ToStringParsed()}." +
                    (splitLines ? Environment.NewLine : " ") +
                    $"The latest release version is v{versionRecord.LatestVersion.ToStringParsed()}. " +
                    $"We are ahead.";
            }
            else
            {
                return $"Currently running v{versionRecord.CurrentVersion.ToStringParsed()}. Up to date.";
            }
        }
    }
}
