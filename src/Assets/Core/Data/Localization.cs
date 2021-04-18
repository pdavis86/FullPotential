using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core.Data
{
    [Serializable]
    public class Localization
    {
        public string Culture;
        public KeyValuePair<string, string>[] Translations;
    }
}
