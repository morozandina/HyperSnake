using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;

    private void FixedUpdate()
    {
        if (target == null)
            return;
        transform.position = Vector3.Lerp(transform.position, target.position, smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, smoothSpeed);
    }
}
