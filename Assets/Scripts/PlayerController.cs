using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 6f;               // Variable de velocidad (criterio B)
    public float acceleration = 40f;       // Suaviza los cambios de velocidad
    public float sprintMultiplier = 1.6f;  // Sprint con Shift
    public float jumpForce = 6f;           // Salto con Espacio

    [Header("Suelo")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundMask = ~0;

    Rigidbody rb;
    Collider col;
    Vector2 input;
    bool sprinting;
    bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsOver) { input = Vector2.zero; return; }

        var k = Keyboard.current;
        if (k == null) return;

        float x = 0f, z = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
        if (k.sKey.isPressed || k.downArrowKey.isPressed)  z -= 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed)    z += 1f;

        input = new Vector2(x, z).normalized;
        sprinting = k.leftShiftKey.isPressed;
        if (k.spaceKey.wasPressedThisFrame) jumpRequested = true;
    }

    void FixedUpdate()
    {
        float targetSpeed = speed * (sprinting ? sprintMultiplier : 1f);
        Vector3 v = rb.linearVelocity;

        Vector3 horizontal = new Vector3(v.x, 0f, v.z);
        Vector3 target = new Vector3(input.x, 0f, input.y) * targetSpeed;
        horizontal = Vector3.MoveTowards(horizontal, target, acceleration * Time.fixedDeltaTime);

        v.x = horizontal.x;
        v.z = horizontal.z;

        // Salto solo si está apoyado (sin salto infinito)
        if (jumpRequested && IsGrounded())
        {
            v.y = jumpForce;
            SoundManager.Play(SoundManager.Sfx.Jump);
        }
        jumpRequested = false;

        rb.linearVelocity = v;
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;
        return Physics.Raycast(b.center, Vector3.down, b.extents.y + groundCheckDistance,
                               groundMask, QueryTriggerInteraction.Ignore);
    }
}
