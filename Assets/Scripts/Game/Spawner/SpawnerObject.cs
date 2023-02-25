using System.Collections.Generic;
using UnityEngine;

namespace Game.Spawner
{
    
    [System.Serializable]
    public class SpawnerObject
    {
        public string name;
        public List<GameObject> spawnPrefabs;
        public int spawnCountMix;
        public int spawnCountMax;
        public int GetSpawnCount => Random.Range(spawnCountMix, spawnCountMax);
    }
}
