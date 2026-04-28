using System.Collections;
using UnityEngine;

public class ChargerEnemy : EnemyBase
{
    private enum ChargerState
    {
        Idle,
        Windup,
        Charging,
        Recover
    }

    [Header("Charge Settings")]
    [SerializeField] private float chargeDistance = 6f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float windupTime = 1.0f;
    [SerializeField] private float recoveryTime = 1.0f;

    [Header("Warning Line")]
    [SerializeField] private LineRenderer warningLine;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Animation")]
    [SerializeField] private ChargerAnimationController animationController;

    private ChargerState currentState = ChargerState.Idle;

    private Vector2 chargeDirection;
    private Vector2 chargeTarget;
    private Coroutine stateRoutine;

    protected override void Awake()
    {
        base.Awake();

        if (animationController == null)
            animationController = GetComponentInChildren<ChargerAnimationController>();
    }

    protected override void Update()
    {
        base.Update();

        switch (currentState)
        {
            case ChargerState.Idle:
                {
                    if (CanSeePlayer())
                    {
                        Vector2 dir = DirectionToPlayer();

                        if (animationController != null)
                            animationController.UpdateAnimation(false, dir);

                        if (stateRoutine == null)
                            stateRoutine = StartCoroutine(Windup());
                    }
                    break;
                }

            case ChargerState.Windup:
                {
                    if (CanSeePlayer())
                    {
                        Vector2 dir = DirectionToPlayer();

                        if (animationController != null)
                            animationController.UpdateAnimation(false, dir);
                    }
                    break;
                }

            case ChargerState.Charging:
                {
                    if (animationController != null)
                        animationController.UpdateAnimation(true, chargeDirection);
                    break;
                }

            case ChargerState.Recover:
                {
                    if (animationController != null)
                        animationController.UpdateAnimation(false, chargeDirection);
                    break;
                }
        }
    }

    private IEnumerator Windup()
    {
        currentState = ChargerState.Windup;

        float timer = 0f;
        warningLine.enabled = true;

        while (timer < windupTime)
        {
            if (!CanSeePlayer())
            {
                warningLine.enabled = false;
                currentState = ChargerState.Idle;
                stateRoutine = null;
                yield break;
            }

            chargeDirection = DirectionToPlayer().normalized;
            chargeTarget = (Vector2)transform.position + chargeDirection * chargeDistance;

            if (animationController != null)
                animationController.UpdateAnimation(false, chargeDirection);

            warningLine.SetPosition(0, transform.position);
            warningLine.SetPosition(1, chargeTarget);

            float progress = timer / windupTime;
            SetLineColor(progress > 0.7f ? dangerColor : normalColor);

            timer += Time.deltaTime;
            yield return null;
        }

        warningLine.enabled = false;
        stateRoutine = StartCoroutine(Charge());
    }

    private IEnumerator Charge()
    {
        currentState = ChargerState.Charging;

        while (Vector2.Distance(rb.position, chargeTarget) > 0.1f)
        {
            Vector2 newPos = Vector2.MoveTowards(
                rb.position,
                chargeTarget,
                chargeSpeed * Time.deltaTime
            );

            rb.MovePosition(newPos);

            if (animationController != null)
                animationController.UpdateAnimation(true, chargeDirection);

            yield return null;
        }

        stateRoutine = StartCoroutine(Recover());
    }

    private IEnumerator Recover()
    {
        currentState = ChargerState.Recover;

        rb.linearVelocity = Vector2.zero;

        if (animationController != null)
            animationController.UpdateAnimation(false, chargeDirection);

        yield return new WaitForSeconds(recoveryTime);

        currentState = ChargerState.Idle;
        stateRoutine = null;
    }

    private void SetLineColor(Color color)
    {
        warningLine.startColor = color;
        warningLine.endColor = color;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (currentState == ChargerState.Charging)
        {
            StopAllCoroutines();
            warningLine.enabled = false;
            stateRoutine = StartCoroutine(Recover());
        }
    }
}