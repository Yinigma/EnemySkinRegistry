using System;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class EnemyRepository
    {
        private IDictionary<string, EnemyInfo> enemyRegistry = new Dictionary<string, EnemyInfo>();

        internal Dictionary<string, EnemyInfo> Enemies => new Dictionary<string, EnemyInfo>(enemyRegistry);

        internal void RegisterEnemy(string enemyType, string name)
        {
            enemyRegistry.Add(enemyType, new EnemyInfo(name, enemyType));
        }

        /*internal EnemyInfo? GetEnemyByType(string enemyType)
        {
            return enemyType != null && enemyRegistry.ContainsKey(enemyType) ? enemyRegistry[enemyType] : null;
        }*/
    }
}