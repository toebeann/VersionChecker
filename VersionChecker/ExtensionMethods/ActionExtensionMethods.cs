using System;
using System.Threading;
using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.ExtensionMethods
{
    internal static class ActionExtensionMethods
    {
        public static Action Debounce(this Action action, float seconds = .5f)
        {
            CancellationTokenSource ctSource = null;

            return async () =>
            {
                ctSource?.Cancel();
                ctSource = new CancellationTokenSource();

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds), ctSource.Token);
                    action();
                }
                catch (TaskCanceledException) { }
            };
        }
    }
}
