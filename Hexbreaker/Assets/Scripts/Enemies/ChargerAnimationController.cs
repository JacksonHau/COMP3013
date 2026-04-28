using UnityEngine;

public class ChargerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Vector2 lastDirection = Vector2.down;
    private string currentStateName = "";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void UpdateAnimation(bool isMoving, Vector2 direction)
    {
        if (animator == null)
            return;

        if (direction.sqrMagnitude > 0.001f)
            lastDirection = direction.normalized;

        string prefix = isMoving ? "Charger_Move_" : "Charger_Idle_";
        string stateName = prefix + GetDirectionName(lastDirection);

        if (stateName == currentStateName)
            return;

        currentStateName = stateName;
        animator.Play(stateName);
    }

    private string GetDirectionName(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? "East" : "West";
        }
        else
        {
            return dir.y > 0 ? "North" : "South";
        }
    }
}