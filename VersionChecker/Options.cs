using SMLHelper.V2.Options;
using UnityEngine;

namespace Straitjacket.Utility
{
    public partial class VersionChecker : MonoBehaviour
    {
        class Options : ModOptions
        {
            public Options() : base("VersionChecker")
            {
                InitEvents();
            }

            private void InitEvents()
            {
                ChoiceChanged += Options_ChoiceChanged;
            }

            public override void BuildModOptions()
            {
                AddChoiceOption("frequency", "Frequency of checks", config.Frequency);
            }

            private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
            {
                switch (e.Id)
                {
                    case "frequency":
                        config.Frequency = (CheckFrequency)e.Index;
                        break;
                }
                config.Save();
            }
        }
    }
}
