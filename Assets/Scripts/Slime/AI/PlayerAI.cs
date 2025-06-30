using System.Collections;
using UnityEngine;

public class PlayerAI : SlimeAI
{
    public static PlayerAI Instance { get; private set; }
    public bool IsSummoning { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected override void LateStart()
    {
        SetBehaviorState(BehaviorState.Idle);
    }

    protected override void HandleBehavior()
    {
        // TODO: Rework to use Update() instead of multiple concurring coroutines

        switch (behaviorState)
        {
            case BehaviorState.Idle:
                // Do nothing
                break;
            case BehaviorState.ChaseAttack:
                StartCoroutine(ChaseTargetCoroutine(defaultBehaviorState, BehaviorState.Attack));
                StartCoroutine(nameof(FollowTargetCoroutine), slimeCombat.CombatStats.AttackRange);
                break;
            case BehaviorState.ChaseEat:
                StartCoroutine(ChaseTargetCoroutine(defaultBehaviorState, BehaviorState.Eat));
                StartCoroutine(nameof(FollowTargetCoroutine), slimeCombat.CombatStats.AttackRange);
                break;
            case BehaviorState.Attack:
                StartCoroutine(nameof(AttackCoroutine));
                break;
            case BehaviorState.Eat:
                StartCoroutine(nameof(EatCoroutine));
                break;
            case BehaviorState.Dead:
                Debug.Log("Slime is dead!");
                break;
        }
    }

    public void SummonSlime()
    {
        IsSummoning = true;
    }

    public void CancelSummonSlime()
    {
        IsSummoning = false;
    }

    private IEnumerator EatCoroutine()
    {
        yield return new WaitForSeconds(1f);

        currentTarget.TryEat();
    }
}