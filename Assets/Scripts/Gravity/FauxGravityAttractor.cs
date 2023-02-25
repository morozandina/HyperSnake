using UnityEngine;

namespace Gravity
{
    public class FauxGravityAttractor : MonoBehaviour
    {
        public float gravity = -9.8f;
        
        public void Attract(Rigidbody body)
        {
            var gravityUp = (body.position - transform.position).normalized;
            var localUp = body.transform.up;
		
            body.AddForce(gravityUp * gravity);
            body.rotation = Quaternion.FromToRotation(localUp,gravityUp) * body.rotation;
        }
        
        private float InputMovement()
        {
            if (Input.GetMouseButton(0))
            {
                var halfScreen = Screen.width / 2;
                var xPos = (Input.mousePosition.x - halfScreen) / halfScreen;
                return xPos > 0 ? 1 : -1;
            }
        
            return 0;
        }
        
        // transform.Rotate(Vector3.up * InputMovement() * rotationSpeed * Time.deltaTime);
    }
}
