using SMLHelper.V2.Json;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using UnityEngine;
using System;

namespace Straitjacket.Utility
{
    public partial class VersionChecker : MonoBehaviour
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

        internal class Config : ConfigFile
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public CheckFrequency Frequency { get; set; } = CheckFrequency.Hourly;

            public DateTime LastChecked { get; set; }
        }
    }
}
