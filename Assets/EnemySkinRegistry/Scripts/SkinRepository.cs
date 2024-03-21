using System;
using System.Collections.Generic;


namespace AntlerShed.SkinRegistry
{

    internal class SkinRepository
    {
        private IDictionary<string, Skin> skins = new Dictionary<string, Skin>();

        internal IDictionary<string, Skin> Skins => new Dictionary<string, Skin>(skins);

        public void RegisterSkin(Skin skin)
        {
            skins.Add(skin.Id, skin);
        }

        public Skin GetSkin(string skinId)
        {
            return skinId != null && skins.ContainsKey(skinId) ? skins[skinId] : null;
        }
    }
}
