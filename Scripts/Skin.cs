using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    /// <summary>
    /// Interface that must be implemented in order to have a skin be submissable to the registry
    /// Factory pattern with some meta-data attached
    /// </summary>
    public interface Skin
    {
        public Skinner CreateSkinner();
        public string Label { get; }
        public string Id { get; }
        public string EnemyId { get; }
        public Texture2D Icon{ get; }

        /*internal Skin(string id = null, Skinner skinner = null, string label=null, string enemyType=null, bool vanilla = false, Texture2D icon = null)
        {
            Id = id;
            Skinner = skinner;
            Label = label;
            EnemyType = enemyType;
            Vanilla = vanilla;
            Icon = icon ?? Texture2D.whiteTexture;
        }*/
    }
}
