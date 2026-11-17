using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boss : MonoBehaviour, IDamageable
{
    public GameObject minionPrefab;
    public int maxHealth = 30;
    public int contactDamage = 2;
    public float speed = 1.6f;
    public float minionInterval = 8f;
    public int minionsPerSummon = 2;

    Rigidbody rb;
    Transform player;
    int health;
    float nextMinionTime;
    float lastContactDamage = -10f;
    bool defeated;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        health = maxHealth;
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        nextMinionTime = Time.time + minionInterval;
        UpdateHealthBar();
    }

    void Update()
    {
        if (defeated || player == null || GameManager.Instance == null || GameManager.Instance.IsOver) return;
        if (minionPrefab != null && Time.time >= nextMinionTime)
        {
            nextMinionTime = Time.time + minionInterval;
            for (int i = 0; i < minionsPerSummon; i++)
            {
                Vector2 circle = Random.insideUnitCircle * 2f;
                Vector3 spawn = transform.position + new Vector3(circle.x, 0f, circle.y);
                Instantiate(minionPrefab, spawn, Quaternion.identity);
            }
        }
    }

    void FixedUpdate()
    {
        if (defeated || player == null || (GameManager.Instance != null && GameManager.Instance.IsOver)) return;

        Vector3 target = ZombieNav.GetTarget(transform.position, player);
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        direction.Normalize();
        Vector3 velocity = rb.linearVelocity;
        velocity.x = direction.x * speed;
        velocity.z = direction.z * speed;
        rb.linearVelocity = velocity;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || Time.time - lastContactDamage < 1f) return;
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;
        lastContactDamage = Time.time;
        playerHealth.Damage(contactDamage);
    }

    public void TakeDamage(int amount)
    {
        if (defeated || amount <= 0) return;
        health = Mathf.Max(0, health - amount);
        UpdateHealthBar();
        Effects.Burst(transform.position + Vector3.up * 2f, new Color(0.75f, 0.2f, 1f), 12, 3f, 0.14f, 0.3f);
        if (health > 0) return;

        defeated = true;
        Effects.Burst(transform.position + Vector3.up, new Color(0.75f, 0.2f, 1f), 50, 6f, 0.25f, 0.8f);
        if (UIManager.Instance != null) UIManager.Instance.HideBossBar();
        if (GameManager.Instance != null) GameManager.Instance.BossDefeated();
        Destroy(gameObject);
    }

    void UpdateHealthBar()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowBossBar("JEFE ZOMBIE", (float)health / Mathf.Max(maxHealth, 1));
    }
}
