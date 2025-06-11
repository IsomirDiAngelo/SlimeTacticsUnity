using System;
using UnityEngine;

public class SlimeAnimationVisual : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walking,
        Attacking,
        Dying,
    }

    private const string IS_DYING_BOOL = "IsDying"; 
    private const string IS_WALKING_BOOL = "IsWalking"; 

    protected SlimeMovement slimeMovement;
    protected SlimeAI slimeAI;
    protected Animator animator;

    private void Start()
    {
        if (TryGetComponent(out slimeMovement))
        {
            slimeMovement.OnWalkingStateChanged += SlimeMovement_OnWalkingStateChanged;
        }
        else
        {
            Debug.LogError("Slime movement component is missing!");
        }

        if (TryGetComponent(out slimeAI))
        {
            slimeAI.OnSlimeDeath += SlimeAI_OnSlimeDeath;
        }
        else
        {
            Debug.LogError("Slime AI component is missing!");
        }

        if (!TryGetComponent(out animator)) Debug.LogError("Animator component is missing!");

    }

    private void SlimeAI_OnSlimeDeath()
    {
        animator.SetBool(IS_DYING_BOOL, true);
    }

    private void SlimeMovement_OnWalkingStateChanged(bool isWalking)
    {
        animator.SetBool(IS_WALKING_BOOL, isWalking);
    }
}