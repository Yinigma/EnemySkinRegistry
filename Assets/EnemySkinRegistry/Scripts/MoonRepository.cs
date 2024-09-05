using System;
using System.Collections.Generic;
using System.Linq;

namespace AntlerShed.SkinRegistry
{
    class MoonRepository
    {
        private IDictionary<string, MoonInfo> moonRegistry = new Dictionary<string, MoonInfo>();

        internal IDictionary<string, MoonInfo> Moons => new Dictionary<string, MoonInfo>(moonRegistry);

        internal ISet<string> MoonTags => new HashSet<string>(tags);

        private ISet<string> tags = new HashSet<string>();

        internal void RegisterMoon(string key, string label, string[] tags)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidMoonIdException(key);
            }
            if (moonRegistry.ContainsKey(key))
            {
                throw new MoonKeyCollisionException(key);
            }
            if (this.tags.Contains(key))
            {
                this.tags.Remove(key);
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Moon \"{key}\" had the same id as an existing tag. The tag is being removed in favor of the moon. Moon names should be prefixed with a number and tags should not have the same names as moons.");
            }
            moonRegistry.Add(key, new MoonInfo(label, key, tags?.ToHashSet() ?? new HashSet<string>()));
            foreach (string tag in tags)
            {
                if (!moonRegistry.ContainsKey(tag))
                {
                    this.tags.Add(tag);
                }
                else
                {
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Tag \"{tag}\" from moon \"{key}\" had the same id as an existing moon. The tag has been skipped in favor of the moon. Moon names should be prefixed with a number and tags should not have the same names as moons.");
                }
            }
        }

        internal MoonInfo? GetMoon(string key)
        {
            return key != null && moonRegistry.ContainsKey(key) ? moonRegistry[key] : null;
        }
    }

    class MoonKeyCollisionException : Exception
    {
        internal MoonKeyCollisionException(string moonId) : base($"Moon or Tag \"{moonId}\" has already been registered.") { }
    }

    class InvalidMoonIdException : Exception
    {
        internal InvalidMoonIdException(string badId) : base($"\"{badId ?? "null"}\" is not a valid moon Id") { }
    }
}