using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    [SerializeField] private PlayerDash dash;

    private Vector2 moveInput;
    private Vector2 mousePos;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Read movement
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // Read mouse position
        mousePos = inputActions.Player.Look.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        HandleMovement();
        //HandleRotation();
    }

    void HandleMovement()
    {
        if (dash != null && dash.IsDashing)
            return;

        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    void HandleRotation()
    {
        Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 aimDir = worldMousePos - rb.position;

        if (aimDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle);
        }
    }
}