using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float speed = 25f;
    public int damage = 1;
    public float lifeTime = 3f;

    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.CompareTag("Player")) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null) target.TakeDamage(damage);

        Effects.Burst(transform.position, new Color(1f, 0.85f, 0.3f), 8, 3f, 0.1f, 0.25f);
        Destroy(gameObject);
    }
}
