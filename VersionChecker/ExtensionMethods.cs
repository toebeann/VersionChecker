﻿using System;

namespace Straitjacket
{
    internal static class ExtensionMethods
    {
        public static string ToStringParsed(this Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (version.Revision > 0)
            {
                return version.ToString();
            }
            else if (version.Build > 0)
            {
                return version.ToString(3);
            }
            else
            {
                return version.ToString(2);
            }
        }
    }
}
