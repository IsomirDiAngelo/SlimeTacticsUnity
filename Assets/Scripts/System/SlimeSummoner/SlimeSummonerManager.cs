using System.Linq;
using UnityEngine;

public class SlimeSummonerManager : MonoBehaviour
{

    public static SlimeSummonerManager Instance { get; private set; }

    [SerializeField] private Transform[] summonableSlimes;

    private Transform[] availableForSummonSlimes;

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

    private void Start()
    {
        if (summonableSlimes.Length > 0)
        {
            availableForSummonSlimes = summonableSlimes.ToArray();
        }
    }


}