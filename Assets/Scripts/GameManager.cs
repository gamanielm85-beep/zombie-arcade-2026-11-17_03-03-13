using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int targetScore = 12;             // Meta mínima de la pauta
    public bool winByScore = true;           // Ganar al llegar a la meta (lo pide la pauta)
    public float winAutoRestartSeconds = 5f; // 0 = reinicia al instante, sin pantalla
    public int score, kills, playerHealth, maxHealth = 5;

    public bool IsOver { get; private set; }

    UIManager ui;
    bool restarting;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        ui = gameObject.AddComponent<UIManager>();
        ui.SetRestartAction(Restart);
    }

    void Start() { RefreshHUD(); }

    public void AddScore(int points, string itemName)
    {
        if (IsOver) return;

        score += points;
        Debug.Log($"Recolectado: {itemName} ({(points >= 0 ? "+" : "")}{points}) | Puntaje acumulado: {score}");

        Color c = points < 0 ? new Color(1f, 0.3f, 0.3f)
                : points >= 3 ? new Color(1f, 0.85f, 0.2f)
                : new Color(1f, 0.6f, 0.15f);
        ui.ShowMessage($"{(points >= 0 ? "+" : "")}{points}  {itemName}", c);
        SoundManager.Play(points >= 3 ? SoundManager.Sfx.Special : points < 0 ? SoundManager.Sfx.Hurt : SoundManager.Sfx.Pickup);

        RefreshHUD();
        if (winByScore && score >= targetScore)
        {
            Debug.Log("¡Meta alcanzada! Reiniciando partida completa...");
            Win(null);
        }
    }

    public void RegisterKill()
    {
        kills++;
        Debug.Log($"Zombie eliminado. Total: {kills}");
        SoundManager.Play(SoundManager.Sfx.ZombieDie);
        RefreshHUD();
    }

    public void SetHealth(int health, int max)
    {
        playerHealth = health;
        maxHealth = max;
        RefreshHUD();
    }

    public void BossDefeated()
    {
        if (IsOver) return;
        Win("¡JEFE DERROTADO!");
    }

    void Win(string title)
    {
        if (IsOver) return;
        IsOver = true;
        SoundManager.Play(SoundManager.Sfx.Win);

        if (winAutoRestartSeconds <= 0f) { Restart(); return; }
        Time.timeScale = 0f;
        ui.ShowEnd(true, score, kills, winAutoRestartSeconds, title);
    }

    public void GameOver()
    {
        if (IsOver) return;
        IsOver = true;
        Debug.Log("Moriste. GAME OVER.");
        SoundManager.Play(SoundManager.Sfx.Lose);
        Time.timeScale = 0f;
        ui.ShowEnd(false, score, kills, -1f);
    }

    public void Restart()
    {
        if (restarting) return;
        restarting = true;
        Time.timeScale = 1f;
        // Recarga TODA la escena: ítems, zombies, oleadas y jugador vuelven al estado inicial
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void RefreshHUD() { ui.UpdateHUD(score, targetScore, playerHealth, maxHealth, kills); }
}
