using QModManager.API;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Subnautica.Logger;

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    internal class VersionChecker : MonoBehaviour
    {
        internal enum CheckFrequency
        {
            Startup,
            Hourly,
            Daily,
            Weekly,
            Monthly,
            Never
        }

        private delegate string ApiKeyCommand(string apiKey = null);

        public const string Version = "1.3.0.2";
        private const string VersionCheckerApiKey = "VersionCheckerApiKey";

        private static string apiKey;
        public static string ApiKey
        {
            get => apiKey ??= PlayerPrefs.HasKey(VersionCheckerApiKey) ? PlayerPrefs.GetString(VersionCheckerApiKey) : null;
            set => apiKey = value;
        }

        private static VersionChecker main;
        public static VersionChecker Main => main ??= new GameObject("VersionChecker").AddComponent<VersionChecker>();

        private bool isRunning = false;
        private bool startupChecked = false;
        private readonly Config Config = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        private readonly Dictionary<IQMod, VersionRecord> CheckedVersions = new Dictionary<IQMod, VersionRecord>();

        public void Check(IQMod qMod, string url)
        {
            if (qMod == null)
            {
                throw new ArgumentNullException("qMod");
            }

            if (CheckedVersions.ContainsKey(qMod))
            {
                return;
            }

            CheckedVersions[qMod] = new VersionRecord(qMod, url);
        }

        public void Check(IQMod qMod, QModGame game, uint modId, string url = null)
        {
            if (qMod == null)
            {
                throw new ArgumentNullException("qMod");
            }

            if (CheckedVersions.TryGetValue(qMod, out VersionRecord record) && record.Game != QModGame.None)
            {
                return;
            }

            CheckedVersions[qMod] = new VersionRecord(qMod, game, modId, url);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void Awake()
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (main != null && main != this)
            {
                Destroy(this);
            }
            else
            {
                main = this;
                gameObject.AddComponent<SceneCleanerPreserve>();
                DontDestroyOnLoad(this);
                SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;

                ConsoleCommandsHandler.Main.RegisterConsoleCommand<ApiKeyCommand>("apikey", SetApiKey);
            }
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!isRunning)
            {
                isRunning = true;
                _ = CheckVersionsAsyncLoop();
                SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            }
        }

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

        private async Task CheckVersionsAsyncLoop()
        {
            await Task.Delay(500);

            while (true)
            {
                _ = Logger.LogDebugAsync("Awaiting next check...");
                await ShouldCheckVersionsAsync();
                _ = Logger.LogDebugAsync("Time to check versions.");

                Config.LastChecked = DateTime.UtcNow;
                Config.Save();

                _ = Logger.LogInfoAsync("Initiating version checks...");

                IEnumerable<Task> updateRecordTasks = CheckedVersions.Values.Select(x => x.UpdateLatestVersionAsync());
                await Task.WhenAll(updateRecordTasks);
                _ = Logger.LogInfoAsync("Version checks complete.");

                _ = PrintOutdatedVersionsAsync();
            }
        }

        private async Task PrintOutdatedVersionsAsync()
        {
            await NoWaitScreenAsync();
            await Task.Delay(1000);

            foreach (var versionRecord in CheckedVersions.Values)
                _ = PrintVersionInfoAsync(versionRecord);
        }

        private async Task PrintVersionInfoAsync(VersionRecord versionRecord)
        {
            switch (versionRecord.State)
            {
                case VersionRecord.VersionState.Outdated:
                    _ = Logger.LogWarningAsync($"{versionRecord.Prefix}{versionRecord.Message(false)}");

                    for (var i = 0; i < 3; i++)
                    {
                        _ = Logger.DisplayMessageAsync($"[<color=#{ColorUtility.ToHtmlStringRGB(versionRecord.Colour)}>" +
                            $"{versionRecord.DisplayName}</color>] {versionRecord.Message(true)}");

                        if (i < 2)
                            await Task.WhenAll(Task.Delay(5000), NoWaitScreenAsync());
                    }
                    break;
                case VersionRecord.VersionState.Unknown:
                    _ = Logger.LogWarningAsync($"{versionRecord.Prefix}{versionRecord.Message(false)}");
                    break;
                case VersionRecord.VersionState.Ahead:
                case VersionRecord.VersionState.Current:
                default:
                    _ = Logger.LogMessageAsync($"{versionRecord.Prefix}{versionRecord.Message(false)}");
                    break;
            }
        }

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
        private async Task SpecificDateTimeAsync(DateTime dateTime, int millisecondsInterval = 1000)
        {
            while (DateTime.UtcNow < dateTime)
                await Task.Delay(millisecondsInterval);
        }

        private async Task NoWaitScreenAsync()
        {
            while (WaitScreen.main?.isShown ?? false)
                await Task.Delay(1000);
        }
    }
}
