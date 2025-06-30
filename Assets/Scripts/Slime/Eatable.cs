using System;
using UnityEngine;

public class Eatable : MonoBehaviour
{
    [SerializeField] private SlimeSummonSO slimeSummonSO;
    [SerializeField] private float vulnerabiltyWindow = 0.2f;

    public event Action<bool> OnEatableStatusChanged;

    private SlimeAI slimeAI;
    private bool isEatable;
    public bool IsEatable
    {
        get { return isEatable; }
        private set
        {
            isEatable = value;
            OnEatableStatusChanged?.Invoke(isEatable);
        }
    }

    private void Start()
    {
        if (TryGetComponent(out slimeAI))
        {
            slimeAI.SlimeCombatComponent.CombatStats.OnHealthChange += SlimeCombatStats_OnHealthChange;
            slimeAI.OnTryEat += Eat;
        }
        else
        {
            Debug.Log("[Eatable] SlimeAI component not found!");
        }
    }

    private void SlimeCombatStats_OnHealthChange(float newHealthAmount)
    {
        float currentHealthPercent = slimeAI.SlimeCombatComponent.CombatStats.CurrentHealth / slimeAI.SlimeCombatComponent.CombatStats.MaxHealth;
        if (currentHealthPercent <= vulnerabiltyWindow)
        {
            IsEatable = true;
        }
        else
        {
            IsEatable = false;
        }
    }

    public void Eat()
    {
        Debug.Log("Eat");
        if (slimeAI.SlimeFaction == SlimeAI.Faction.Ally)
        {
            SlimeSummonerManager.Instance.UnlockSlimeSummon(slimeSummonSO);
            SlimeSummonerManager.Instance.DespawnSlime(transform);
        }
        else
        {
            if (IsEatable)
            {
                Debug.Log("Eatable");
                SlimeSummonerManager.Instance.UnlockSlimeSummon(slimeSummonSO);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Target is not eatable!");
            }
        }

    }
}