using System;
using System.Collections;
using System.Collections.Generic;
using Gravity;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game.Meteor
{
    [Serializable]
    public class MeteorControl
    {
        public float startDelay, betweenDelay, count;
    }
    public class MeteorManager : MonoBehaviour
    {
        public List<MeteorControl> meteorControls = new List<MeteorControl>();
        public float startDelay, betweenDelay, count;
        public GameObject meteorPrefab;
        [SerializeField] private FauxGravityAttractor fauxGravityAttractor;
        [SerializeField] private Transform planet;

        private List<GameObject> _meteors = new List<GameObject>();

        public void StartSpawning()
        {
            StartCoroutine(MeteorControl());
        }

        private IEnumerator MeteorControl()
        {
            GameUIManager.Instance.MeteorTimer((int)startDelay);
            yield return new WaitForSeconds(startDelay);

            while (true)
            {
                if (_meteors.Count == 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        SpawnMeteor();
                        yield return new WaitForSeconds(.2f);
                    }

                    GameUIManager.Instance.MeteorTimer((int)betweenDelay);
                    yield return new WaitForSeconds(betweenDelay);
                }
                else
                {
                    foreach (GameObject meta in _meteors)
                    {
                        SpawnMeteorPool(meta);
                    }
                }
            }
        }

        private void SpawnMeteor()
        {
            // Starting Point (Planet Center)
            var spawnPoint = planet.position;

            // Random Outward Direction
            var randomDir = Random.onUnitSphere;

            // Modify Point to edge of planet
            spawnPoint += (randomDir * (planet.localScale.y * 2)); 

            // Outward Rotation for Object
            var spawnRotation = (spawnPoint - planet.position).normalized;

            // Spawn And Rotate into Place
            var addSpawn = Instantiate(meteorPrefab, spawnPoint, new Quaternion(0,0,0,0));
            addSpawn.transform.rotation = Quaternion.FromToRotation(addSpawn.transform.up, spawnRotation) * addSpawn.transform.rotation;
            addSpawn.transform.eulerAngles -= new Vector3(90, 0, 0);
            addSpawn.GetComponent<FauxGravityBody>().planet = fauxGravityAttractor;
        }
        
        private void SpawnMeteorPool(GameObject meteor)
        {
            // Starting Point (Planet Center)
            var spawnPoint = planet.position;

            // Random Outward Direction
            var randomDir = Random.onUnitSphere;

            // Modify Point to edge of planet
            spawnPoint += (randomDir * (planet.localScale.y * 2)); 

            // Outward Rotation for Object
            var spawnRotation = (spawnPoint - planet.position).normalized;

            // Spawn And Rotate into Place
            meteor.transform.position = spawnPoint;
            meteor.transform.rotation = Quaternion.FromToRotation(meteor.transform.up, spawnRotation) * meteor.transform.rotation;
            meteor.transform.eulerAngles -= new Vector3(90, 0, 0);
            meteor.SetActive(true);
        }
    }
}
