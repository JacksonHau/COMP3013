using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;

    [Header("I-Frames")]
    [SerializeField] private float invulnerableTime = 0.2f;

    [SerializeField] private TrailRenderer dashTrail;
    private Rigidbody2D rb;
    private Health health;
    private PlayerInputActions inputActions;

    private bool isDashing = false;
    public bool IsDashing => isDashing;

    private bool canDash = true;

    private Vector2 lastMoveDirection = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        inputActions = new PlayerInputActions();

        if (dashTrail == null)
        {
            dashTrail = GetComponentInChildren<TrailRenderer>();
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Dash.performed += OnDash;
    }

    private void OnDisable()
    {
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Disable();
    }

    private void Update()
    {
        // Track last movement direction (for dash when idle)
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (!canDash || isDashing)
            return;

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;

        Vector2 dashDir = lastMoveDirection.normalized;

        // enable i-frames
        if (health != null)
            health.SetInvulnerable(true);

        if (dashTrail != null)
        {
            dashTrail.Clear();
            dashTrail.emitting = true;
        }

        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.linearVelocity = dashDir * dashForce;

            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false; 

        float slowDownTime = 0.1f; 
        float slowTimer = 0f;

        Vector2 startVelocity = rb.linearVelocity;

        while (slowTimer < slowDownTime)
        {
            float t = slowTimer / slowDownTime;
            t = t * t; // ease-out
            rb.linearVelocity = Vector2.Lerp(startVelocity, Vector2.zero, t);
            slowTimer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (dashTrail != null)
            dashTrail.emitting = false;

        float remainingIFrames = Mathf.Max(0f, invulnerableTime - dashDuration);
        if (remainingIFrames > 0f)
            yield return new WaitForSeconds(remainingIFrames);

        if (health != null)
            health.SetInvulnerable(false);

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
}