using System;
using System.Collections.Generic;
using System.Linq;
using Game.Meteor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game.Spawner
{
    public class Spawner : MonoBehaviour
    {
        public AudioClip[] backgroundMusic;
        public static PlanetSettings planetSettings;
        public static Action<GameObject> AppleSpawn;
        public static Action<GameObject> PropsSpawn;
        private Transform _planet, _decals;
        public Material planetMaterial;
        public GameObject apple;
        public MeteorManager meteorManager;
        public Transform snake;

        private void Awake()
        {
            _planet = transform.GetChild(0);
            _decals = transform.GetChild(1);
            Sound.Instance.PlayMusic(backgroundMusic[Random.Range(0, backgroundMusic.Length)]);
            
            _planet.localScale = Vector3.one * planetSettings.size;
            _decals.localScale = Vector3.one * planetSettings.size;
            snake.position = new Vector3(0, 0, (planetSettings.size / 2) + 5);
            
            StartSpawn();

            AppleSpawn += SpawnApple;
            PropsSpawn += SpawnProps;
        }

        private void OnDestroy()
        {
            AppleSpawn -= SpawnApple;
            PropsSpawn -= SpawnProps;
        }

        private void StartSpawn()
        {
            meteorManager.startDelay = planetSettings.meteorStartDelay;
            meteorManager.betweenDelay = planetSettings.meteorBetweenDelay;
            meteorManager.count = planetSettings.meteorCount;
            meteorManager.StartSpawning();
            
            planetMaterial.SetColor("_color1", planetSettings.planetColors[0]);
            planetMaterial.SetColor("_color2", planetSettings.planetColors[1]);
            foreach (var spawner in planetSettings.spawnerObjects)
            {
                for (var i = 0; i <= spawner.GetSpawnCount; i++)
                    SpawnPrefab(spawner.spawnPrefabs, spawner.name != "Props");
            }

            for (var i = 0; i <= planetSettings.totalStarCount; i++)
                SpawnApple();
        }

        private void SpawnPrefab(IReadOnlyList<GameObject> toSpawn, bool isStatic)
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
            addSpawn.isStatic = isStatic;
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

        private void SpawnApple(GameObject apple)
        {
            apple.SetActive(false);
            // Starting Point (Planet Center)
            var spawnPoint = _planet.position;

            // Random Outward Direction
            var randomDir = Random.onUnitSphere;

            // Modify Point to edge of planet
            spawnPoint += (randomDir * (_planet.localScale.y / 2 + .3f)); 

            // Outward Rotation for Object
            var spawnRotation = (spawnPoint - _planet.position).normalized;

            // Spawn And Rotate into Place
            apple.transform.position = spawnPoint;
            apple.transform.rotation = Quaternion.FromToRotation(apple.transform.up, spawnRotation) * apple.transform.rotation;
            apple.SetActive(true);
        }
        
        private void SpawnProps(GameObject props)
        {
            props.SetActive(false);
            // Starting Point (Planet Center)
            var spawnPoint = _planet.position;

            // Random Outward Direction
            var randomDir = Random.onUnitSphere;

            // Modify Point to edge of planet
            spawnPoint += (randomDir * (_planet.localScale.y / 2 - .1f));

            // Outward Rotation for Object
            var spawnRotation = (spawnPoint - _planet.position).normalized;

            // Spawn And Rotate into Place
            props.transform.position = spawnPoint;
            props.transform.rotation = Quaternion.FromToRotation(props.transform.up, spawnRotation) * props.transform.rotation;
            props.SetActive(true);
        }
    }
}
