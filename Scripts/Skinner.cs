using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    public interface Skinner
    {
        /// <summary>
        /// Interface call to apply the skin.
        /// </summary>
        /// <param name="enemy">The enemy's root GameObject (i.e. the owner of the AI component)</param>
        void Apply(GameObject enemy);

        /// <summary>
        /// Interface call to remove the skin. Should undo everything done in "Apply"
        /// </summary>
        /// <param name="enemy">The enemy's root GameObject (i.e. the owner of the AI component)</param>
        void Remove(GameObject enemy);
    }
}