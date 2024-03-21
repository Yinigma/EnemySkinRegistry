using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class MoonRepository
    {
        private IDictionary<string, MoonInfo> moonRegistry = new Dictionary<string, MoonInfo>();

        internal IDictionary<string, MoonInfo> Moons => new Dictionary<string, MoonInfo>(moonRegistry);

        internal void RegisterMoon(string key, string label)
        {
            moonRegistry.Add(key, new MoonInfo(label, key));
        }

        internal MoonInfo? GetMoon(string key)
        {
            return key != null && moonRegistry.ContainsKey(key) ? moonRegistry[key] : null;
            
        }
    }
}