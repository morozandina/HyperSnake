using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game.Spawner
{
    public class Spawner : MonoBehaviour
    {
        public static Action AppleSpawn;
        private Transform _planet;
        public Material planetMaterial;
        public GameObject apple;
        public PlanetSettings planetSettings;

        private void Awake()
        {
            _planet = transform.GetChild(0);
            StartSpawn();

            AppleSpawn += SpawnApple;
        }

        private void OnDestroy()
        {
            AppleSpawn -= SpawnApple;
        }

        private void StartSpawn()
        {
            planetMaterial.SetColor("_color1", planetSettings.planetColors[0]);
            planetMaterial.SetColor("_color2", planetSettings.planetColors[1]);
            foreach (var spawner in planetSettings.spawnerObjects)
            {
                for (var i = 0; i <= spawner.GetSpawnCount; i++)
                    SpawnPrefab(spawner.spawnPrefabs);
            }

            StaticBatchingUtility.Combine(_planet.gameObject);
            SpawnApple();
        }

        public void SpawnPrefab(IReadOnlyList<GameObject> toSpawn)
        {
            // Pick Random Object
            var prefabInd = Random.Range(0, toSpawn.Count);
        
            // Starting Point (Planet Center)
            var spawnPoint = _planet.position;

            // Random Outward Direction
            var randomDir = Random.onUnitSphere;

            // Modify Point to edge of planet
            spawnPoint += (randomDir * (_planet.localScale.y / 2 - .1f)); 

            // Outward Rotation for Object
            var spawnRotation = (spawnPoint - _planet.position).normalized;

            // Spawn And Rotate into Place
            var addSpawn = Instantiate(toSpawn[prefabInd], spawnPoint, new Quaternion(0,0,0,0));
            addSpawn.transform.rotation = Quaternion.FromToRotation(addSpawn.transform.up, spawnRotation) * addSpawn.transform.rotation;
            
            // Parent to Globe
            addSpawn.transform.parent = _planet;
            addSpawn.isStatic = true;
        }
        
        private void SpawnApple()
        {
            // Starting Point (Planet Center)
            var spawnPoint = _planet.position;

            // Random Outward Direction
            var randomDir = Random.onUnitSphere;

            // Modify Point to edge of planet
            spawnPoint += (randomDir * (_planet.localScale.y / 2 + .3f)); 

            // Outward Rotation for Object
            var spawnRotation = (spawnPoint - _planet.position).normalized;

            // Spawn And Rotate into Place
            var addSpawn = Instantiate(apple, spawnPoint, new Quaternion(0,0,0,0));
            addSpawn.transform.rotation = Quaternion.FromToRotation(addSpawn.transform.up, spawnRotation) * addSpawn.transform.rotation;

        }
    }
}
