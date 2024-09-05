using LethalLevelLoader;
using System.Collections.Generic;
using System.Linq;

namespace AntlerShed.SkinRegistry
{
    internal class LethalLevelLoaderCompatibility
    {
        internal static void RegisterLLLMaps()
        {
            HashSet<string> alreadyRegistered = new HashSet<string>();
            foreach (ExtendedLevel level in PatchedContent.ExtendedLevels)
            {
                if (level.SelectableLevel != null)
                {
                    EnemySkinRegistry.RegisterMoon(level.SelectableLevel.PlanetName, level.NumberlessPlanetName, level.ContentTags.Select((tag) => tag.contentTagName.ToLower()).ToArray(), null);
                    alreadyRegistered.Add(level.SelectableLevel.PlanetName);
                }
            }
            foreach (ExtendedMod mod in PatchedContent.ExtendedMods)
            {
                foreach (ExtendedLevel level in mod.ExtendedLevels)
                {
                    if (level.SelectableLevel != null && !alreadyRegistered.Contains(level.SelectableLevel.PlanetName))
                    {
                        EnemySkinRegistry.RegisterMoon(level.SelectableLevel.PlanetName, level.NumberlessPlanetName, level.ContentTags.Select((tag)=>tag.contentTagName.ToLower()).ToArray(), null);
                        alreadyRegistered.Add(level.SelectableLevel.PlanetName);
                    }
                }
            }
        }
    }
}