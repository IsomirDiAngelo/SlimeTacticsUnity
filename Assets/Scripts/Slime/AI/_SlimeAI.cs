using System;
using System.Collections;
using UnityEngine;

public class SlimeAI : MonoBehaviour, IInteractable
{
    protected enum BehaviorState
    {
        Idle,
        Follow,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    public enum Faction
    {
        Ally,
        Enemy,
        Neutral
    }

    public event Action OnSlimeDeath;

    protected SlimeMovement slimeMovement;
    protected SlimeCombat slimeCombat;
    public SlimeCombat SlimeCombatComponent => slimeCombat;
    protected BehaviorState behaviorState;

    protected SlimeAI currentTarget;

    [SerializeField] protected Faction slimeFaction;
    public Faction SlimeFaction { get { return slimeFaction; } }
    [SerializeField] protected Vector3[] patrolPathWaypoints;
    [SerializeField] protected BehaviorState defaultBehaviorState;

    private int patrolPathIndex;

    private void Start()
    {
        if (!TryGetComponent(out slimeMovement)) Debug.LogError("Slime movement component missing!");
        if (!TryGetComponent(out slimeCombat))
        {
            Debug.LogError("Slime combat component missing!");
        }
        // slimeMovement.OnPathComplete += SlimeMovement_OnPathComplete;
        slimeCombat.CombatStats.OnHealthChange += SlimeCombatStats_OnHealthChange;

        InputManager.Instance.OnMove += InputManager_OnMove;

        patrolPathIndex = 0;

        LateStart();
    }

    // LateStart is meant to be overriden by child classes to add additional parameters to initialize during Start()
    protected virtual void LateStart()
    {
        SetBehaviorState(defaultBehaviorState);
    }

    private void SlimeCombatStats_OnHealthChange(float newHealthAmount)
    {
        if (newHealthAmount == 0)
        {
            SetBehaviorState(BehaviorState.Dead);
            OnSlimeDeath?.Invoke();
        }
    }

    protected void SetBehaviorState(BehaviorState newBehaviorState)
    {
        OnBehaviorStateChanged(newBehaviorState);
        behaviorState = newBehaviorState;
    }

    protected void SetCurrentTarget(SlimeAI target)
    {
        currentTarget = target;
    }

    public bool IsDead()
    {
        return behaviorState == BehaviorState.Dead;
    }

    private void OnBehaviorStateChanged(BehaviorState newBehaviorState)
    {
        StopAllCoroutines();
        slimeMovement.Stop();
        slimeCombat.CancelAttack();

        // TODO: Rework to use Update() instead of multiple concurring coroutines

        switch (newBehaviorState)
        {
            case BehaviorState.Idle:
                // Do nothing
                break;
            case BehaviorState.Follow:
                SetCurrentTarget(PlayerAI.Instance);
                StartCoroutine(nameof(ScoutTargetCoroutine));
                StartCoroutine(nameof(FollowTargetCoroutine), slimeCombat.CombatStats.TargetRange);
                break;
            case BehaviorState.Patrol:
                StartCoroutine(nameof(ScoutTargetCoroutine));
                StartCoroutine(nameof(FollowPatrolPathCoroutine));
                break;
            case BehaviorState.Chase:
                StartCoroutine(nameof(ChaseTargetCoroutine), defaultBehaviorState); // Default to previous state when target becomes unreachable
                StartCoroutine(nameof(FollowTargetCoroutine), slimeCombat.CombatStats.AttackRange);
                break;
            case BehaviorState.Attack:
                StartCoroutine(nameof(AttackCoroutine));
                break;
            case BehaviorState.Dead:
                Debug.Log("Slime is dead!");
                break;
        }
    }

    public Faction GetEnemyFaction()
    {
        switch (SlimeFaction)
        {
            case Faction.Enemy:
                return Faction.Ally;
            default:
                return Faction.Enemy;
        }
    }

    private void TryMoveTo(Vector3 worldPosition) {
        SetBehaviorState(BehaviorState.Idle);
        slimeMovement.RequestAndFollowPath(worldPosition, false);
    }

    private IEnumerator FollowPatrolPathCoroutine()
    {
        int patrolPathIncrement = 1;

        while (true)
        {
            if (!slimeMovement.IsWalking)
            {
                // Don't recalculate path if already walking on patrol

                if (patrolPathWaypoints != null && patrolPathWaypoints.Length > 0)
                {
                    // A custom patrol path was defined

                    if (patrolPathIndex == patrolPathWaypoints.Length - 1 && patrolPathIncrement == 1 || patrolPathIndex == 0 && patrolPathIncrement == -1)
                    {
                        // Progress in the opposite direction in the path array
                        patrolPathIncrement = -patrolPathIncrement;
                    }

                    patrolPathIndex += patrolPathIncrement;
                    slimeMovement.RequestAndFollowPath(patrolPathWaypoints[patrolPathIndex], false);
                }
                else
                {
                    // No predetermined path was defined
                    slimeMovement.RequestAndFollowPath(new Vector3(0, 0, 0), false);
                }
                yield return null;
            }

            float waitTime = 2f;
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator ScoutTargetCoroutine()
    {
        while (true)
        {
            if (slimeCombat.TryAcquireClosestTarget(out SlimeAI newTarget, GetEnemyFaction()))
            {
                currentTarget = newTarget;

                if (slimeCombat.IsTargetInAttackRange(currentTarget))
                {
                    SetBehaviorState(BehaviorState.Attack);
                }
                else
                {
                    SetBehaviorState(BehaviorState.Chase);
                }
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator ChaseTargetCoroutine(BehaviorState defaultBehaviorState)
    {

        while (true)
        {
            if (currentTarget == null || currentTarget.IsDead())
            {
                SetBehaviorState(defaultBehaviorState);
                yield break;
            }

            if (slimeCombat.IsTargetInAttackRange(currentTarget))
            {
                SetBehaviorState(BehaviorState.Attack);
                yield break;
            }
            else
            {
                if (slimeCombat.TryAcquireClosestTarget(out SlimeAI newTarget, GetEnemyFaction()) && newTarget != currentTarget)
                {
                    currentTarget = newTarget;
                }
            }

            yield return null;
        }
    }

    private IEnumerator FollowTargetCoroutine(float followDistance)
    {
        float recalculatePathThreshold = 0.1f;
        Vector3 lastTargetPosition = transform.position;

        while (currentTarget != null && !currentTarget.IsDead())
        {
            if (Vector3.Distance(lastTargetPosition, currentTarget.transform.position) > recalculatePathThreshold)
            {
                slimeMovement.RequestAndFollowPath(currentTarget.transform.position, true, followDistance);
                lastTargetPosition = currentTarget.transform.position;
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            if (currentTarget == null || currentTarget.IsDead() || !slimeCombat.IsTargetInAttackRange(currentTarget))
            {
                SetBehaviorState(BehaviorState.Chase);
                yield break;
            }

            if (!slimeCombat.IsAttacking)
            {
                slimeMovement.Stop();
                slimeCombat.StartAttack(currentTarget.SlimeCombatComponent);
            }
            yield return null;
        }
    }



    // Player commands
    public void Interact(SlimeAI actor)
    {
        if (!actor.IsDead() && actor.SlimeFaction == GetEnemyFaction())
        {
            actor.SetCurrentTarget(this);
            actor.SetBehaviorState(BehaviorState.Chase);
        }
    }

    private void InputManager_OnMove(Vector3 worldPosition, SlimeAI actor)
    {
        if (actor == this && !IsDead() && SlimeFaction == Faction.Ally)
        {
            actor.TryMoveTo(worldPosition);
        }
    }

}