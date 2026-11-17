using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 12f, -9f);
    public float smoothTime = 0.12f;
    public float lookHeight = 0.8f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        Vector3 lookPoint = target.position + Vector3.up * lookHeight;
        Vector3 direction = lookPoint - transform.position;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
