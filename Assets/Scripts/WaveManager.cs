using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public GameObject zombiePrefab;
    public GameObject bossPrefab;
    public Transform[] spawnPoints;
    public int totalWaves = 4;
    public int startingZombies = 3;
    public int extraZombiesPerWave = 2;
    public float spawnInterval = 1.2f;
    public float breakBetweenWaves = 3f;

    int currentWave;
    bool started;

    IEnumerator Start()
    {
        if (started) yield break;
        started = true;
        if (zombiePrefab == null || totalWaves <= 0)
        {
            Debug.LogError("WaveManager: asigna un prefab de zombie y un total de oleadas mayor que cero.", this);
            yield break;
        }

        totalWaves = Mathf.Max(1, totalWaves);
        for (currentWave = 1; currentWave <= totalWaves; currentWave++)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsOver) yield break;

            bool bossWave = currentWave == totalWaves && bossPrefab != null;
            UIManager.Instance?.SetWaveInfo(bossWave ? "OLEADA JEFE" : $"OLEADA {currentWave}/{totalWaves}",
                bossWave ? "Derrota al jefe" : "Sobrevive a los zombies");
            UIManager.Instance?.ShowBanner(bossWave ? "¡JEFE FINAL!" : $"OLEADA {currentWave}",
                bossWave ? new Color(0.85f, 0.4f, 1f) : new Color(1f, 0.75f, 0.25f));
            SoundManager.Play(bossWave ? SoundManager.Sfx.Roar : SoundManager.Sfx.Wave);

            if (bossWave)
            {
                Vector3 spawn = GetSpawnPosition();
                Instantiate(bossPrefab, spawn, Quaternion.identity);
            }
            else
            {
                int count = Mathf.Max(1, startingZombies + (currentWave - 1) * extraZombiesPerWave);
                for (int i = 0; i < count; i++)
                {
                    if (GameManager.Instance != null && GameManager.Instance.IsOver) yield break;
                    Vector3 spawn = GetSpawnPosition();
                    Instantiate(zombiePrefab, spawn, Quaternion.identity);
                    if (spawnInterval > 0f) yield return new WaitForSeconds(spawnInterval);
                }
            }

            while (HasLivingEnemies())
            {
                if (GameManager.Instance != null && GameManager.Instance.IsOver) yield break;
                yield return new WaitForSeconds(0.5f);
            }

            if (currentWave < totalWaves && breakBetweenWaves > 0f)
                yield return new WaitForSeconds(breakBetweenWaves);
        }
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = Random.Range(0, spawnPoints.Length);
            if (spawnPoints[index] != null) return spawnPoints[index].position;
            foreach (Transform point in spawnPoints)
                if (point != null) return point.position;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position + Vector3.back * 8f : Vector3.back * 8f;
    }

    bool HasLivingEnemies()
    {
        if (Zombie.AliveCount > 0) return true;
        return FindFirstObjectByType<Boss>() != null;
    }
}
