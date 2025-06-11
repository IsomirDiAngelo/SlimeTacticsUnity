using System;
using System.Collections;
using UnityEngine;

public class SlimeCombat : MonoBehaviour
{
    [Serializable]
    public class Stats
    {
        public event Action OnManaFull;
        public event Action<float> OnHealthChange;

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float MaxMana { get; private set; }
        public float CurrentMana { get; private set; }
        public float AttackDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float AttackRange { get; private set; }
        public float TargetRange { get; private set; }

        public Stats(SlimeBaseStatsSO slimeBaseStatsSO)
        {
            MaxHealth = slimeBaseStatsSO.Health;
            CurrentHealth = slimeBaseStatsSO.Health;
            MaxMana = slimeBaseStatsSO.Mana;
            CurrentMana = slimeBaseStatsSO.StartingMana;
            AttackDamage = slimeBaseStatsSO.AttackDamage;
            AttackSpeed = slimeBaseStatsSO.AttackSpeed;
            AttackRange = slimeBaseStatsSO.AttackRange;
            TargetRange = slimeBaseStatsSO.TargetRange;
        }

        public void TakeDamage(float amount)
        {
            float newHealthAmount = CurrentHealth - amount;
            if (newHealthAmount <= 0)
            {
                CurrentHealth = 0;
            }
            else
            {
                CurrentHealth = newHealthAmount;
            }
            OnHealthChange?.Invoke(CurrentHealth);
        }

        public void GainMana(int manaAmount)
        {
            float newManaAmount = CurrentMana + manaAmount;
            if (newManaAmount >= MaxMana)
            {
                CurrentMana = MaxMana;
                OnManaFull?.Invoke();
            }
            else
            {
                CurrentMana = newManaAmount;
            }
        }
    }

    public event Action<bool> OnAttackingStateChange;
    public event Action<bool> OnCastingStateChange;
    public event Action OnHitTaken;

    [SerializeField] protected SlimeBaseStatsSO slimeBaseStatsSO;
    public Stats CombatStats { get; private set; }

    private bool isAttacking;
    public bool IsAttacking
    {
        get
        {
            return isAttacking;
        }

        private set
        {
            isAttacking = value;
            OnAttackingStateChange?.Invoke(isAttacking);
        }
    }
    private bool isCasting;

    private void Start()
    {
        if (slimeBaseStatsSO != null)
        {
            CombatStats = new(slimeBaseStatsSO);
        }
        else
        {
            Debug.LogError("Missing base stats SO!");
        }
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        StopCoroutine(nameof(Attack));
    }

    public void StartAttack(SlimeCombat target)
    {
        if (!IsAttacking)
        {
            StartCoroutine(nameof(Attack), target);
        }
    }

    public bool IsDead()
    {
        return CombatStats.CurrentHealth <= 0;
    }

    public bool IsTargetInAttackRange(SlimeCombat target)
    {
        float errorMargin = 0.25f;
        return Math.Abs(Vector3.Distance(target.transform.position, transform.position) - CombatStats.AttackRange) <= errorMargin;
    }

    public bool TryAcquireClosestTarget(out SlimeCombat target, SlimeAI.Faction faction)
    {
        target = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, CombatStats.TargetRange);
        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out SlimeAI collidingSlime) && collidingSlime.SlimeFaction == faction)
            {
                float distance = Vector3.Distance(transform.position, collidingSlime.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    if (collidingSlime.TryGetComponent(out SlimeCombat newTarget) && !newTarget.IsDead()) {
                        target = newTarget; 
                    }
                }
            }
        }

        return target != null;
    }

    private IEnumerator Attack(SlimeCombat target)
    {
        IsAttacking = true;

        if (CombatStats.AttackSpeed == 0)
        {
            Debug.LogError("Attack speed is set to 0!");
            yield break;
        }
        yield return new WaitForSeconds(1 / CombatStats.AttackSpeed);
        IsAttacking = false;

        int manaPerAttack = 10;
        CombatStats.GainMana(manaPerAttack);

        target.CombatStats.TakeDamage(CombatStats.AttackDamage);
        target.OnHitTaken?.Invoke();
    }

}