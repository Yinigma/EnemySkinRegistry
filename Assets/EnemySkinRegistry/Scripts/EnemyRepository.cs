using System;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    internal class EnemyRepository
    {
        private IDictionary<string, EnemyInfo> enemyRegistry = new Dictionary<string, EnemyInfo>();

        internal Dictionary<string, EnemyInfo> Enemies => new Dictionary<string, EnemyInfo>(enemyRegistry);

        internal void RegisterEnemy(string enemyType, string name, SpawnLocation location)
        {
            if (string.IsNullOrEmpty(enemyType))
            {
                throw new InvalidEnemyTypeException(enemyType);
            }
            if (enemyRegistry.ContainsKey(enemyType))
            {
                throw new DuplicateEnemyException(enemyType);
            }
            enemyRegistry.Add(enemyType, new EnemyInfo(name, enemyType, location));
        }
    }

    internal class DuplicateEnemyException : Exception
    {
        internal DuplicateEnemyException(string enemyType) : base($"Enemy with type \"{enemyType}\" has already been registered.") { }
    }

    internal class InvalidEnemyTypeException : Exception
    {
        internal InvalidEnemyTypeException(string badType) : base($"\"{badType ?? "null"}\" is not a valid enemy type.") { }
    }
}