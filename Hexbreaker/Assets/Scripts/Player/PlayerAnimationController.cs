using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private Vector2 aimDirection = Vector2.down;
    private Vector2 lastMoveDirection = Vector2.down;

    private PlayerInputActions inputActions;
    private bool isAttacking;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.FireLeft.performed += OnAttack;
        inputActions.Player.FireRight.performed += OnAttack;
    }

    private void OnDisable()
    {
        inputActions.Player.FireLeft.performed -= OnAttack;
        inputActions.Player.FireRight.performed -= OnAttack;
        inputActions.Disable();
    }

    private float attackLockTimer;

    private void Update()
    {
        if (attackLockTimer > 0f)
        {
            attackLockTimer -= Time.deltaTime;
            return;
        }

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        aimDirection = (mouseWorld - (Vector2)transform.position).normalized;

        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDirection = moveInput.normalized;

        UpdateAnimation();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        attackLockTimer = 0.15f;
        PlayDirectionalAnimation("Player_Attack", aimDirection);
    }

    private void UpdateAnimation()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            PlayDirectionalAnimation("Player_Run", moveInput);
        }
        else
        {
            PlayDirectionalAnimation("Player_Idle", lastMoveDirection);
        }
    }

    private void PlayDirectionalAnimation(string baseState, Vector2 dir)
    {
        string suffix = GetDirectionSuffix(dir);
        animator.Play(baseState + "_" + suffix);
    }

    private string GetDirectionSuffix(Vector2 dir)
    {
        dir.Normalize();

        if (dir.y > 0.5f)
        {
            if (dir.x > 0.5f) return "NorthEast";
            if (dir.x < -0.5f) return "NorthWest";
            return "North";
        }

        if (dir.y < -0.5f)
        {
            if (dir.x > 0.5f) return "SouthEast";
            if (dir.x < -0.5f) return "SouthWest";
            return "South";
        }

        if (dir.x > 0.5f) return "East";
        return "West";
    }
}