using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invulnerableTime = 1f;

    int health;
    float lastHit = -99f;

    void Start()
    {
        health = maxHealth;
        GameManager.Instance.SetHealth(health, maxHealth);
    }

    public void Damage(int amount)
    {
        if (GameManager.Instance.IsOver) return;
        if (Time.time - lastHit < invulnerableTime) return;
        lastHit = Time.time;

        health -= amount;
        Debug.Log($"¡Un zombie te mordió! Vida: {health}");
        SoundManager.Play(SoundManager.Sfx.Hurt);
        Effects.Burst(transform.position, new Color(0.9f, 0.1f, 0.1f), 18, 4f, 0.18f, 0.5f);
        if (UIManager.Instance != null) UIManager.Instance.FlashDamage();

        GameManager.Instance.SetHealth(health, maxHealth);
        if (health <= 0) GameManager.Instance.GameOver();
    }
}
