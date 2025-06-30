using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlimeSummonerManager : MonoBehaviour
{
    public static SlimeSummonerManager Instance { get; private set; }

    [SerializeField] private SlimeSummonSO[] slimeSummonSOArray;
    [SerializeField] private int summoningCapacity = 1;
    [SerializeField] private float summoningRange = 5f;

    private List<SlimeSummonSO> availableSlimeSummonSOList;
    private SlimeSummonSO selectedSlimeSummonSO;
    private List<Transform> summonedSlimes;

    public event Action<List<SlimeSummonSO>> OnSlimeSummonListChanged;

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
        availableSlimeSummonSOList = new();
        summonedSlimes = new();
        selectedSlimeSummonSO = null;

        SlimeSummonCardUI.OnClick += SlimeSummonCardUI_OnClick;
        InputManager.Instance.OnSpawn += InputManager_OnSpawn;
    }

    public void UnlockSlimeSummon(SlimeSummonSO slimeSummonSO)
    {
        availableSlimeSummonSOList.Add(slimeSummonSO);
        if (!slimeSummonSOArray.Contains(slimeSummonSO))
        {
            slimeSummonSOArray.Append(slimeSummonSO);
        }
        OnSlimeSummonListChanged?.Invoke(availableSlimeSummonSOList);
    }

    public void LockSlimeSummon(SlimeSummonSO slimeSummonSO)
    {
        availableSlimeSummonSOList.Remove(slimeSummonSO);
        OnSlimeSummonListChanged?.Invoke(availableSlimeSummonSOList);
    }

    public void SetSummoningCapacity(int capacity)
    {
        summoningCapacity = capacity;
    }

    public void DespawnSlime(Transform slimeToDespawn)
    {
        if (summonedSlimes.Contains(slimeToDespawn) )
        {
            summonedSlimes.Remove(slimeToDespawn);
        }

        Destroy(slimeToDespawn.gameObject);
    }

    private void InputManager_OnSpawn(Vector3 worldPosition)
    {
        if (PlayerAI.Instance.IsSummoning)
        {
            if (summonedSlimes.Count < summoningCapacity)
            {
                summonedSlimes.Add(Instantiate(selectedSlimeSummonSO.SlimeSummonPrefab, worldPosition, Quaternion.identity));
                PlayerAI.Instance.CancelSummonSlime();

                LockSlimeSummon(selectedSlimeSummonSO);
                selectedSlimeSummonSO = null;
            }
        }
    }

    private void SlimeSummonCardUI_OnClick(SlimeSummonSO slimeSummonSO)
    {
        PlayerAI.Instance.SummonSlime();
        selectedSlimeSummonSO = slimeSummonSO;
    }

}