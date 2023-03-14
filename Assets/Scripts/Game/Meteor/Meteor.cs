using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Meteor
{
    public class Meteor : MonoBehaviour
    {
        public ParticleSystem boom;
        public DecalProjector decalProjector;
        public AudioClip collisionSound;

        private void Start()
        {
            decalProjector.size = new Vector3(6.00f, 6.00f, 50.00f);
            StartCoroutine(ChangeSpeed(4.5f));
        }

        private IEnumerator ChangeSpeed(float duration )
        {
            var elapsed = 0.0f;
            while (elapsed < duration )
            {
                decalProjector.size = new Vector3(6.00f, 6.00f, Mathf.Lerp( 50, 20, elapsed / duration ));
                decalProjector.pivot = new Vector3(0.00f, 0.00f, decalProjector.size.z / 2);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Sound.Instance.PlaySound(collisionSound);
            Instantiate(boom, transform.position, transform.rotation);
            gameObject.SetActive(false);
        }
    }
}
