using UnityEngine;

public class PlayerAI : SlimeAI
{
    protected override void LateStart()
    {
        SetBehaviorState(BehaviorState.Idle);
    }
}