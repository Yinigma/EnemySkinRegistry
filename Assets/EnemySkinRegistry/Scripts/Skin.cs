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
        /// <summary>
        /// The name of the skin as it will show up in the configuration menu
        /// </summary>
        public string Label { get; }
        /// <summary>
        /// The unique id of the skin. To help enforce uniqueness, it's recommended that ids follow the pattern "<AuthorName>.<SkinName>"
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The unique id string of the enemy type the Skin will operate on
        /// </summary>
        public string EnemyId { get; }
        /// <summary>
        /// Used as the skin's icon so it can be previewed in the configuration menu
        /// </summary>
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

    public interface NestSkin
    {
        public string SkinId { get; }

        public Skinner CreateNestSkinner();
    }
}
