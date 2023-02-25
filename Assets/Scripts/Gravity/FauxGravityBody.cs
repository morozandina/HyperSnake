using UnityEngine;

namespace Gravity
{
    public class FauxGravityBody : MonoBehaviour
    {
        public FauxGravityAttractor planet;
        private Rigidbody _rigidbody;
	
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody> ();

            // Disable rigidbody gravity and rotation as this is simulated in GravityAttractor script
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
	
        private void FixedUpdate()
        {
            // Allow this body to be influenced by planet's gravity
            planet.Attract(_rigidbody);
        }
    }
}
