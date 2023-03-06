using System;
using UnityEngine;

namespace Game.Spawner
{
    public class StarPositionFix : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Obstacle")) return;
            
            Spawner.AppleSpawn?.Invoke();
            Destroy(gameObject);
        }
    }
}
