using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallRespawn : MonoBehaviour
{
    public float fallLimitY = -10f;
    Vector3 startPosition;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y < fallLimitY)
        {
            rb.linearVelocity = Vector3.zero;   // sin impulso residual
            rb.angularVelocity = Vector3.zero;
            transform.position = startPosition;
            Debug.Log("Caíste al vacío: reaparecés en el punto inicial.");
            SoundManager.Play(SoundManager.Sfx.Respawn);
            Effects.Burst(startPosition, new Color(0.3f, 0.6f, 1f), 25, 4f, 0.2f, 0.6f);
        }
    }
}
