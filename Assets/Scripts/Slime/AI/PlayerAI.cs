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

    public void SummonSlime()
    {
        IsSummoning = true;
    }

    public void CancelSummonSlime()
    {
        IsSummoning = false;
    }
}