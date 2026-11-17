using UnityEngine;

// Va en los prefabs. Cada prefab tiene su propia velocidad de giro.
public class Collectible : MonoBehaviour
{
    public float rotationSpeed = 90f;   // Normal: 90 | Especial: 240
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;

    float baseY, phase;

    void Start()
    {
        baseY = transform.position.y;
        phase = Random.value * 6.28f;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
        transform.position = p;
    }
}
