using System;
using System.Collections.Generic;


namespace AntlerShed.SkinRegistry
{

    internal class SkinRepository
    {
        private IDictionary<string, Skin> skins = new Dictionary<string, Skin>();

        internal IDictionary<string, Skin> Skins => new Dictionary<string, Skin>(skins);

        private IDictionary<string, NestSkin> nestSkins = new Dictionary<string, NestSkin>();

        internal IDictionary<string, NestSkin> NestSkins => new Dictionary<string, NestSkin>(nestSkins);

        public void RegisterSkin(Skin skin)
        {
            if (string.IsNullOrEmpty(skin.Id))
            {
                throw new InvalidSkinIdException(skin.Id);
            }
            if (skins.ContainsKey(skin.Id))
            {
                throw new DuplicateSkinException(skin.Id);
            }
            skins.Add(skin.Id, skin);
        }

        public Skin GetSkin(string skinId)
        {
            return skinId != null && skins.ContainsKey(skinId) ? skins[skinId] : null;
        }

        public void RegisterNestSkin(NestSkin nestSkin)
        {
            if (string.IsNullOrEmpty(nestSkin.SkinId))
            {
                throw new InvalidSkinIdException(nestSkin.SkinId);
            }
            if(!skins.ContainsKey(nestSkin.SkinId))
            {
                throw new MissingSkinException(nestSkin.SkinId);
            }
            if (nestSkins.ContainsKey(nestSkin.SkinId))
            {
                throw new DuplicateSkinException(nestSkin.SkinId);
            }
            nestSkins.Add(nestSkin.SkinId, nestSkin);
        }

        public NestSkin GetNestSkin(string nestSkinId)
        {
            return nestSkinId != null && nestSkins.ContainsKey(nestSkinId) ? nestSkins[nestSkinId] : null;
        }
    }

    internal class MissingSkinException : Exception
    {
        internal MissingSkinException(string skinId) : base($"Attempet to register a Nest Skin with non-existent base skin id \"{skinId}.\" Make sure you're registering the main skin before the nest skin.") { }
    }

    internal class DuplicateSkinException : Exception
    {
        internal DuplicateSkinException(string skinId) : base($"Skin with id \"{skinId}\" has already been registered.") { }
    }

    internal class InvalidSkinIdException : Exception
    {
        internal InvalidSkinIdException(string badId) : base($"\"{badId ?? "null"}\" is not a valid skin Id.") { }
    }
}
