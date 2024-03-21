using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class SessionState
    {
        private System.Random randomNumberGenerator = new System.Random(13);
        public string LevelId { get; private set; }
        private List<SkinInstance> activeSkinners = new List<SkinInstance>();

        internal void ClearActiveSkinners()
        {
            activeSkinners.Clear();
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

        internal List<GameObject> GetSkinnedEnemies()
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

        internal void SetRandomNumberGenerator(int seed)
        {
            randomNumberGenerator = new System.Random(seed);
        }

        internal void SetCurrentLevel(string levelId)
        {
            LevelId = levelId;
        }

        internal float GetRandom()
        {
            return (float) randomNumberGenerator.NextDouble();
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