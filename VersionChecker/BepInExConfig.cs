using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.QMod
{
    using ExtensionMethods;
    using NexusAPI;

    internal static class BepInExConfig
    {
        private readonly static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "VersionChecker.cfg"), true,
            new BepInPlugin(Constants.QModId, Constants.QModDisplayName, Constants.Version));

        public static class Nexus
        {
            public static ConfigEntry<string> ApiKey { get; } = configFile.Bind(nameof(Nexus), nameof(ApiKey), string.Empty,
                    new ConfigDescription("Your Nexus API key.", null, new ConfigurationManagerAttributes { IsAdvanced = true }));

            static Nexus()
            {
                Validate.Main.ApiKey = ApiKey.Value;
                ApiKey.SettingChanged += ApiKey_SettingChanged;
            }

            private readonly static Action apiKeyChanged = () =>
            {
                ApiKey.Value = ApiKey.Value.Trim();

                if (!string.IsNullOrWhiteSpace(ApiKey.Value))
                {
                    _ = ValidateApiKeyAsync();
                }
            };
            private readonly static Action apiKeyChangedWrapper = apiKeyChanged.Debounce(.33f);

            private static void ApiKey_SettingChanged(object _, EventArgs __)
            {
                apiKeyChangedWrapper();
            }

            private static async Task ValidateApiKeyAsync()
            {
                try
                {
                    Validate.Main.ApiKey = ApiKey.Value;
                    await Validate.GetAsync();
                }
                catch
                {
                    ApiKey.Value = ApiKey.DefaultValue as string;
                }
            }
        }

        /// <summary>
        /// Hack to provide support for BepInEx.ConfigurationManager when config is loaded from outside a plugin
        /// </summary>
        internal static void BindPlugin()
        {
            _ = BindPluginAsync();
        }

        private static async Task BindPluginAsync()
        {
            PluginInfo pluginInfo;

            while (!Chainloader.PluginInfos.TryGetValue(Constants.QModId, out pluginInfo))
            {
                await Task.Delay(100);
            }

            var traverse = Traverse.Create(pluginInfo.Instance);
            traverse.Field($"<{nameof(BaseUnityPlugin.Config)}>k__BackingField").SetValue(configFile);
        }
    }
}
