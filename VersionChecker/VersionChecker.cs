using QModManager.API;
using QModManager.Utility;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Utility.VersionChecker
{
    /// <summary>
    /// An API for checking the client is running the latest version of a mod, informing the user if it is not.
    /// </summary>
    public class VersionChecker : MonoBehaviourSingleton<VersionChecker>
    {
        internal const string Version = "1.2.0.0";

        internal enum CheckFrequency
        {
            Startup,
            Hourly,
            Daily,
            Weekly,
            Monthly,
            Never
        }

        internal static VersionChecker GetSingleton() => Main ?? new GameObject("VersionChecker").AddComponent<VersionChecker>();

        internal static Dictionary<IQMod, VersionRecord> CheckedVersions = new Dictionary<IQMod, VersionRecord>();
        internal static IVersionParser VersionParser { get; } = new VersionParser();

        /// <summary>
        /// Entry point for the VersionChecker API when the latest version number is stored in plain text at a given URL as an Assembly Version.
        /// </summary>
        /// <param name="URL">The URL at which the plain text file containing the latest version number can be found.</param>
        /// <param name="currentVersion">A <see cref="Version"/> describing the current version number of the mod that is installed.
        /// Optional. By default, will be retrieved from the mod.json or the compiled assembly.</param>
        /// <param name="displayName">The display name to use for the mod. Optional. By default, will be retrieved from the mod.json or the mod's
        /// compiled assembly.</param>
        [Obsolete("This method is deprecated in favour of adding the VersionChecker object to your mod.json. Please see the wiki for details.", true)]
        public static void Check(string URL, Version currentVersion = null, string displayName = null)
        {
            GetSingleton();

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

            string prefix = assembly == Assembly.GetAssembly(typeof(VersionChecker))
                ? string.Empty
                : $"[{displayName}] ";

            if (currentVersion == null)
            {

                Logger.LogError($"{prefix}There was an error retrieving the current version.");
                return;
            }

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = assembly,
                DisplayName = displayName,
                Colour = GetColour(),
                URL = URL,
                CurrentVersion = currentVersion,
                UpdateAsync = async () =>
                {
                    Version version = await GetLatestVersionAsync(URL);
                    CheckedVersions[qMod].LatestVersion = version;
                }
            };
            Logger.LogInfo($"{prefix}Currently running v{currentVersion}.");
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
        [Obsolete("This method is deprecated in favour of adding the VersionChecker object to your mod.json. Please see the wiki for details.", true)]
        public static void Check<TJsonObject>(string URL, string versionProperty = "Version", Version currentVersion = null, string displayName = null)
            where TJsonObject : class
        {
            GetSingleton();

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

            string prefix = assembly == Assembly.GetAssembly(typeof(VersionChecker))
                ? string.Empty
                : $"[{displayName}] ";

            if (currentVersion == null)
            {
                Logger.LogError($"{prefix}There was an error retrieving the current version.");
                return;
            }

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = assembly,
                DisplayName = displayName,
                Colour = GetColour(),
                URL = URL,
                CurrentVersion = currentVersion,
                UpdateAsync = async () =>
                {
                    PropertyInfo versionPropertyInfo = typeof(ModJson).GetProperty(versionProperty);
                    if (versionPropertyInfo == null)
                        throw new InvalidOperationException($"Property {versionProperty} not found in type {typeof(TJsonObject)}");

                    Version version = await GetLatestVersionAsync<TJsonObject>(URL, versionPropertyInfo);
                    CheckedVersions[qMod].LatestVersion = version;
                }
            };
            Logger.LogInfo($"{prefix}Currently running v{currentVersion}.");
        }

        internal static void Check(string URL, IQMod qMod)
        {
            GetSingleton();

            if (qMod == null)
                throw new ArgumentNullException("qMod");

            if (CheckedVersions.ContainsKey(qMod))
                return;

            string prefix = qMod.LoadedAssembly == Assembly.GetAssembly(typeof(VersionChecker))
                ? string.Empty
                : $"[{qMod.DisplayName}] ";

            if (qMod.ParsedVersion == null)
            {
                Logger.LogError($"{prefix}There was an error retrieving the current version: QModManager failed to parse.");
                return;
            }

            string versionProperty = "Version";
            PropertyInfo versionPropertyInfo = typeof(ModJson).GetProperty(versionProperty);

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = qMod.LoadedAssembly,
                DisplayName = qMod.DisplayName,
                Colour = GetColour(),
                URL = URL,
                CurrentVersion = qMod.ParsedVersion,
                UpdateAsync = async () =>
                {
                    if (versionPropertyInfo == null)
                        throw new InvalidOperationException($"Property {versionProperty} not found in type {typeof(ModJson)}");

                    Version version = await GetLatestVersionAsync<ModJson>(URL, versionPropertyInfo);
                    CheckedVersions[qMod].LatestVersion = version;
                }
            };
            Logger.LogInfo($"{prefix}Currently running v{qMod.ParsedVersion}.");
        }

        internal static void Check(QModGame game, uint modId, IQMod qMod, string URL = null)
        {
            GetSingleton();

            if (qMod == null)
                throw new ArgumentNullException("qMod");

            if (CheckedVersions.ContainsKey(qMod))
                return;

            string prefix = qMod.LoadedAssembly == Assembly.GetAssembly(typeof(VersionChecker))
                ? string.Empty
                : $"[{qMod.DisplayName}] ";

            if (qMod.ParsedVersion == null)
            {
                Logger.LogError($"{prefix}There was an error retrieving the current version: QModManager failed to parse.");
                return;
            }

            string versionProperty = "Version";
            PropertyInfo versionPropertyInfo = typeof(ModJson).GetProperty(versionProperty);

            var versionRecord = CheckedVersions[qMod] = new VersionRecord
            {
                Assembly = qMod.LoadedAssembly,
                DisplayName = qMod.DisplayName,
                Colour = GetColour(),
                CurrentVersion = qMod.ParsedVersion,
                Game = game,
                ModId = modId,
                URL = URL
            };
            versionRecord.UpdateAsync = async () =>
            {
                string apiKey = string.IsNullOrWhiteSpace(ApiKey)
                ? (ApiKey = PlayerPrefs.HasKey(VersionCheckerApiKey)
                    ? PlayerPrefs.GetString(VersionCheckerApiKey)
                    : null)
                : ApiKey;

                try
                {
                    Version version = await GetLatestVersionAsync(versionRecord, apiKey);
                    CheckedVersions[qMod].LatestVersion = version;
                }
                catch (Exception)
                {
                    Version version;

                    if (apiKey != null)
                    {
                        try
                        {
                            version = await GetLatestVersionAsync(versionRecord);
                            CheckedVersions[qMod].LatestVersion = version;
                            return;
                        }
                        catch (Exception) { }
                    }

                    if (URL == null)
                        throw new InvalidOperationException("Could not retrieve version from Nexus Mods API and no VersionChecker github URL is defined as a fallback.");

                    if (versionPropertyInfo == null)
                        throw new InvalidOperationException($"Property {versionProperty} not found in type {typeof(ModJson)}");

                    version = await GetLatestVersionAsync<ModJson>(URL, versionPropertyInfo);
                    CheckedVersions[qMod].LatestVersion = version;
                }
            };
        }

        internal static async Task<Version> GetLatestVersionAsync(string URL)
        {
            var text = await Networking.ReadAllTextAsync(URL);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return VersionParser.GetVersion(text.Trim());
            }

            throw new NullReferenceException();
        }

        internal static async Task<Version> GetLatestVersionAsync<TJsonObject>(string URL, PropertyInfo versionPropertyInfo)
            where TJsonObject : class
        {
            if (versionPropertyInfo.PropertyType != typeof(string))
            {
                throw new ArgumentException("A property of Type string is required.", "versionProperty");
            }

            TJsonObject JSON = await Networking.ReadJSONAsync<TJsonObject>(URL);
            return VersionParser.GetVersion((string)versionPropertyInfo.GetValue(JSON, null));
        }

        internal static async Task<Version> GetLatestVersionAsync(VersionRecord versionRecord, string nexusApiKey = null)
        {
            if (nexusApiKey == null)
            {
                string url = versionRecord.VersionCheckerAPIModUrl;
                VersionCheckerAPI.ModJson JSON = await Networking.ReadJSONAsync<VersionCheckerAPI.ModJson>(url);
                return VersionParser.GetVersion(JSON.Version);
            }
            else
            {
                string url = versionRecord.NexusAPIModUrl;
                Dictionary<string, string> headers = new Dictionary<string, string> { ["apikey"] = nexusApiKey };
                NexusAPI.ModJson JSON = await Networking.ReadJSONAsync<NexusAPI.ModJson>(url, headers);
                return VersionParser.GetVersion(JSON.Version);
            }
        }

        internal static Config Config = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        /// <summary>
        /// Called from Awake for the singleton instance of VersionChecker
        /// </summary>
        protected override void SingletonAwake()
        {
            DontDestroyOnLoad(this);
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            ConsoleCommandsHandler.Main.RegisterConsoleCommand<ApiKeyCommand>("apikey", SetApiKey);
        }

        private delegate string ApiKeyCommand(string apiKey = null);
        private const string VersionCheckerApiKey = "VersionCheckerApiKey";
        internal static string ApiKey;

        private string SetApiKey(string apiKey = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                if (PlayerPrefs.HasKey(VersionCheckerApiKey))
                {
                    apiKey = PlayerPrefs.GetString(VersionCheckerApiKey);
                    return $"Nexus API key ending in {apiKey.Substring(Math.Max(0, apiKey.Length - 4))} is set.";
                }
                else
                {
                    return "Nexus API key not set.";
                }
            }
            else if (apiKey.ToLowerInvariant() == "unset")
            {
                PlayerPrefs.DeleteKey(VersionCheckerApiKey);
                return "Nexus API key unset.";
            }
            else
            {
                PlayerPrefs.SetString(VersionCheckerApiKey, ApiKey = apiKey.Trim());
                Config.LastChecked = default;
                return "Nexus API key set.";
            }
        }

        private void OnDestroy() => StopAllCoroutines();

        private bool IsRunning = false;
        private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Main != null && !Main.IsRunning)
            {
                if (scene.name == "StartScreen")
                {
                    Main.IsRunning = true;
                    Task.Run(() => Main.CheckVersionsAsyncLoop());
                    SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
                }
            }
        }

        private static readonly Color[] assemblyColours = new Color[]
        {
            new Color(0, 1, 1), new Color(.65f, .165f, .165f), new Color(0, .5f, 0), new Color(.68f, .85f, .9f), new Color(0, .5f, .5f),
            new Color(0, 1, 0), new Color(1, 0, 1), new Color(.5f, .5f, 0), new Color(1, .65f, 0), new Color(1, 1, 0)
        };
        private static List<Color> assignedColours = new List<Color>();
        private static Color GetColour()
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

        private async Task CheckVersionsAsyncLoop()
        {
            while (true)
            {
                LogDebugs.Add("Awaiting next check...");
                await ShouldCheckVersionsAsync();
                LogDebugs.Add("Time to check versions.");

                Config.LastChecked = DateTime.UtcNow;
                Config.Save();

                LogInfos.Add("Initiating version checks...");

                IEnumerable<Task> updateRecordTasks = CheckedVersions.Values.Select(x => x.UpdateLatestVersionAsync());
                await Task.WhenAll(updateRecordTasks);
                LogInfos.Add("Version checks complete.");

                _ = Task.Run(() => PrintOutdatedVersionsAsync());
            }
        }

        private async Task PrintOutdatedVersionsAsync()
        {
            await NoWaitScreenAsync();
            await Task.Delay(1000);

            foreach (var versionRecord in CheckedVersions.Values)
                _ = Task.Run(() => PrintVersionInfoAsync(versionRecord));
        }

        private async Task PrintVersionInfoAsync(VersionRecord versionRecord)
        {
            string prefix = versionRecord.Assembly == Assembly.GetAssembly(typeof(VersionChecker))
                    ? string.Empty
                    : $"[{versionRecord.DisplayName}] ";

            switch (versionRecord.State)
            {
                case VersionRecord.VersionState.Outdated:
                    LogWarnings.Add($"{prefix}{versionRecord.Message(false)}");

                    for (var i = 0; i < 3; i++)
                    {
                        displayMessages.Add($"[<color=#{ColorUtility.ToHtmlStringRGB(versionRecord.Colour)}>" +
                            $"{versionRecord.DisplayName}</color>] {versionRecord.Message(true)}");

                        if (i < 2)
                            await Task.WhenAll(Task.Delay(5000), NoWaitScreenAsync());
                    }
                    break;
                case VersionRecord.VersionState.Unknown:
                    LogWarnings.Add($"{prefix}{versionRecord.Message(false)}");
                    break;
                case VersionRecord.VersionState.Ahead:
                case VersionRecord.VersionState.Current:
                default:
                    LogMessages.Add($"{prefix}{versionRecord.Message(false)}");
                    break;
            }
        }

        internal List<string> LogDebugs = new List<string>();
        internal List<string> LogInfos = new List<string>();
        internal List<string> LogMessages = new List<string>();
        internal List<string> LogWarnings = new List<string>();
        internal List<string> LogErrors = new List<string>();
        private List<string> displayMessages = new List<string>();
        private void Update()
        {
            if (displayMessages.Any())
            {
                foreach (var message in displayMessages)
                    ErrorMessage.AddError(message);

                displayMessages.Clear();
            }

            if (LogDebugs.Any())
            {
                foreach (var debug in LogDebugs)
                    Logger.LogDebug(debug);

                LogDebugs.Clear();
            }

            if (LogInfos.Any())
            {
                foreach (var info in LogInfos)
                    Logger.LogInfo(info);

                LogInfos.Clear();
            }

            if (LogMessages.Any())
            {
                foreach (var message in LogMessages)
                    Logger.LogMessage(message);

                LogMessages.Clear();
            }

            if (LogWarnings.Any())
            {
                foreach (var warning in LogWarnings)
                    Logger.LogWarning(warning);

                LogWarnings.Clear();
            }

            if (LogErrors.Any())
            {
                foreach (var error in LogErrors)
                    Logger.LogError(error);

                LogErrors.Clear();
            }
        }

        private bool startupChecked = false;
        private async Task ShouldCheckVersionsAsync()
        {
            if (Config.Frequency == CheckFrequency.Startup && !startupChecked)
            {
                startupChecked = true;
                return;
            }

            bool shouldCheck = false;

            while (!shouldCheck)
            {
                switch (Config.Frequency)
                {
                    default:
                    case CheckFrequency.Never:
                        shouldCheck = false;
                        break;
                    case CheckFrequency.Hourly:
                        await SpecificDateTimeAsync(Config.LastChecked.AddHours(1), 60000);
                        shouldCheck = true;
                        break;
                    case CheckFrequency.Daily:
                        await SpecificDateTimeAsync(Config.LastChecked.AddDays(1), 3600000);
                        shouldCheck = true;
                        break;
                    case CheckFrequency.Weekly:
                        await SpecificDateTimeAsync(Config.LastChecked.AddDays(7), 3600000);
                        shouldCheck = true;
                        break;
                    case CheckFrequency.Monthly:
                        await SpecificDateTimeAsync(Config.LastChecked.AddMonths(1), 3600000);
                        shouldCheck = true;
                        break;
                }

                if (!shouldCheck)
                    await Task.Delay(60000);
            }
        }
        private async Task SpecificDateTimeAsync(DateTime dateTime, int delay = 1000)
        {
            while (DateTime.UtcNow < dateTime)
                await Task.Delay(delay);
        }

        private async Task NoWaitScreenAsync()
        {
            while (WaitScreen.main?.isShown ?? false)
                await Task.Delay(1000);
        }
    }
}
