using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class SessionState
    {
        private List<SkinInstance> activeSkinners = new List<SkinInstance>();
        private Dictionary<EnemyAINestSpawnObject, string> activeNestSkins = new Dictionary<EnemyAINestSpawnObject, string>();
        /// <summary>
        /// Some enemies have their nest deleted before they finish spawning, so the skin id that was on their nest has to be saved
        /// </summary>
        private Dictionary<GameObject, string> stagedSkins = new Dictionary<GameObject, string>();

        internal void ClearState()
        {
            activeSkinners.Clear();
            stagedSkins.Clear();
            activeNestSkins.Clear();
        }

        internal void AddSkinner(GameObject enemyInstance, string skinId, string enemyType, Skinner skinner)
        {
            if(enemyInstance!=null && skinner != null && enemyType != null)
            {
                activeSkinners.Add(new SkinInstance(enemyInstance, skinId, enemyType, skinner));
            }
        }

        internal void RemoveInstance(GameObject enemyInstance)
        {
            int index = activeSkinners.FindIndex((active) => active.SkinnedObject.Equals(enemyInstance));
            if (index != -1)
            {
                activeSkinners.RemoveAt(index);
            }
        }

        internal void ClearSkinner(GameObject enemyInstance)
        {
            int index = activeSkinners.FindIndex((active) => active.SkinnedObject.Equals(enemyInstance));
            if(index != -1)
            {
                activeSkinners[index] = new SkinInstance(activeSkinners[index].SkinnedObject, null, activeSkinners[index].EnemyId, new DummySkinner());
            }
        }

        internal List<GameObject> GetSkinnedObject()
        {
            return activeSkinners.Select((active)=>active.SkinnedObject).ToList();
        }

        internal string GetEnemyType(GameObject skinnedInstance)
        {
            int index = activeSkinners.FindIndex((active) => active.SkinnedObject.Equals(skinnedInstance));
            if (index != -1)
            {
                return activeSkinners[index].EnemyId;
            }
            return null;
        }

        internal Skinner GetSkinner(GameObject skinnedInstance)
        {
            int index = activeSkinners.FindIndex((active) => active.SkinnedObject.Equals(skinnedInstance));
            if (index != -1)
            {
                return activeSkinners[index].SkinnerInstance;
            }
            return null;
        }

        internal string GetSkinId(GameObject skinnedInstance)
        {
            int index = activeSkinners.FindIndex((active) => active.SkinnedObject.Equals(skinnedInstance));
            if (index != -1)
            {
                return activeSkinners[index].SkinId;
            }
            return null;
        }

        internal void AddSkinNest(EnemyAINestSpawnObject nest, string skinId)
        {
            if (nest != null)
            {
                activeNestSkins.Add(nest, skinId);
            }
        }

        internal void AddNest(EnemyAINestSpawnObject nest)
        {
            if (nest != null)
            {
                activeNestSkins.Add(nest, null);
            }
        }

        internal void StageSkinForSpawn(EnemyAINestSpawnObject nest, GameObject enemy)
        {
            if(enemy != null && nest != null)
            {
                if (activeNestSkins.ContainsKey(nest))
                {
                    stagedSkins.Add(enemy, activeNestSkins[nest]);
                }
            }
        }

        internal bool SpawnedFromNest(GameObject enemy)
        {
            return stagedSkins.ContainsKey(enemy);
        }

        internal string RetrieveStagedSkin(GameObject enemy)
        {
            if (enemy != null)
            {
                if (stagedSkins.ContainsKey(enemy))
                {
                    return stagedSkins[enemy];
                }
            }
            return null;
        }

        private struct XORShift32
        {
            internal uint Value { get; private set; }
            internal XORShift32(uint seed)
            {
                Value = seed;
            }

            internal void Next()
            {
                Value ^= Value << 13;
                Value ^= Value >> 17;
                Value ^= Value << 5;
            }

            internal float AsFloat()
            {
                return ((float)(Value%4096)) / 4096.0f;
            }
        }
    }


    struct SkinInstance
    {
        internal string EnemyId { get; }
        internal string SkinId { get; } //null if no skin
        internal Skinner SkinnerInstance { get; } //dummy skinner if no skin
        internal GameObject SkinnedObject { get; }

        internal SkinInstance(GameObject skinnedObject, string skinId, string enemyId, Skinner skinner)
        {
            SkinnedObject = skinnedObject;
            EnemyId = enemyId;
            SkinnerInstance = skinner;
            SkinId = skinId;
        }
    }

}