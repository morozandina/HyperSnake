using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Spawner
{
    public class ObjectPositionFix : MonoBehaviour
    {
        public float minis;

        private void Awake()
        {
            transform.localPosition -= new Vector3(0, minis, 0);
        }
    }
}
