using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    public float amount = 1.2f;
    public float speed = 8f;

    Light l;
    float baseIntensity, seed;

    void Awake()
    {
        l = GetComponent<Light>();
        if (l != null) baseIntensity = l.intensity;
        seed = Random.value * 100f;
    }

    void Update()
    {
        if (l == null) return;
        l.intensity = baseIntensity + (Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f) * 2f * amount;
    }
}
