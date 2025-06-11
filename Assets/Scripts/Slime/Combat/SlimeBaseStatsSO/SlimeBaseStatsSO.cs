using UnityEngine;

[CreateAssetMenu()]
public class SlimeBaseStatsSO : ScriptableObject
{
    [SerializeField] private int health;
    [SerializeField] private int mana;
    [SerializeField] private int startingMana;
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] private float targetRange;

    public int Health { get { return health; } private set { health = value; } }
    public int Mana { get { return mana; } private set { mana = value; } }
    public int StartingMana { get { return startingMana; } private set { startingMana = value; } }
    public int AttackDamage { get { return attackDamage; } private set { attackDamage = value; } }
    public float AttackSpeed { get { return attackSpeed; } private set { attackSpeed = value; } }
    public float AttackRange { get { return attackRange; } private set { attackRange = value; } }
    public float TargetRange { get { return targetRange; } private set { targetRange = value; } }
}