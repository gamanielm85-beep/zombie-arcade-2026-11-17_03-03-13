using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform muzzle;          // Objeto hijo vacío en la punta del arma
    public float fireRate = 0.25f;

    Camera cam;
    float nextShot;

    void Start() { cam = Camera.main; }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsOver) return;

        var mouse = Mouse.current;
        if (mouse == null || cam == null) return;

        // Apuntar hacia el mouse sobre un plano a la altura del jugador
        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        Plane plane = new Plane(Vector3.up, transform.position);
        if (plane.Raycast(ray, out float dist))
        {
            Vector3 look = ray.GetPoint(dist) - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(look);
        }

        if (mouse.leftButton.isPressed && Time.time >= nextShot)
        {
            nextShot = Time.time + fireRate;
            Transform origin = muzzle != null ? muzzle : transform;
            Instantiate(bulletPrefab, origin.position, origin.rotation);
            Effects.Burst(origin.position, new Color(1f, 0.9f, 0.4f), 8, 3f, 0.12f, 0.2f);
            SoundManager.Play(SoundManager.Sfx.Shoot, 0.8f);
        }
    }
}
