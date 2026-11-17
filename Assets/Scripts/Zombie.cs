using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

[RequireComponent(typeof(Rigidbody))]
public class Zombie : MonoBehaviour, IDamageable
{
    public float speed = 2.5f;
    public int health = 3;
    public int contactDamage = 1;

    public static int AliveCount;

    Rigidbody rb;
    Transform player;
    float groanTimer;

    void OnEnable()  { AliveCount++; }
    void OnDisable() { AliveCount--; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        groanTimer = Random.Range(3f, 8f);
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // Lo llama el WaveManager para ajustar dificultad y tipo de zombie
    public void Configure(float newSpeed, int newHealth, float scale, Color? tint)
    {
        speed = newSpeed;
        health = newHealth;
        transform.localScale = Vector3.one * scale;

        if (tint.HasValue)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetColor("_BaseColor", tint.Value);
            mpb.SetColor("_Color", tint.Value);
            foreach (var r in GetComponentsInChildren<Renderer>())
                if (r.sharedMaterial != null && r.sharedMaterial.name.StartsWith("M_Zombie"))
                    r.SetPropertyBlock(mpb);
        }
    }

    void Update()
    {
        if (player == null) return;
        groanTimer -= Time.deltaTime;
        if (groanTimer <= 0f)
        {
            groanTimer = Random.Range(4f, 9f);
            if (Vector3.Distance(transform.position, player.position) < 12f)
                SoundManager.Play(SoundManager.Sfx.Groan, 0.5f);
        }
    }

    void FixedUpdate()
    {
        if (transform.position.y < -10f) { Destroy(gameObject); return; }
        if (player == null) return;

        Vector3 target = ZombieNav.GetTarget(transform.position, player);
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        dir.Normalize();

        Vector3 v = rb.linearVelocity;
        v.x = dir.x * speed;
        v.z = dir.z * speed;
        rb.linearVelocity = v;

        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);
    }

    void OnCollisionStay(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            var hp = c.gameObject.GetComponent<PlayerHealth>();
            if (hp != null) hp.Damage(contactDamage);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Effects.Burst(transform.position + Vector3.up, new Color(0.3f, 0.9f, 0.2f), 8, 3f, 0.12f, 0.35f);

        if (health <= 0)
        {
            Effects.Burst(transform.position + Vector3.up, new Color(0.3f, 0.9f, 0.2f), 30, 5f, 0.22f, 0.7f);
            Effects.Burst(transform.position + Vector3.up, new Color(0.7f, 0.05f, 0.05f), 18, 4f, 0.15f, 0.6f);
            GameManager.Instance.RegisterKill();
            Destroy(gameObject);
        }
    }
}
