using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlimeSummonerManager : MonoBehaviour
{
    public static SlimeSummonerManager Instance { get; private set; }

    [SerializeField] private SlimeSummonSO[] slimeSummonSOArray;

    private List<SlimeSummonSO> availableSlimeSummonSOList;
    private SlimeSummonSO selectedSlimeSummonSO;

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
        // TODO: Implement unlocking of different slime summons
        if (slimeSummonSOArray.Length > 0)
        {
            availableSlimeSummonSOList = slimeSummonSOArray.ToList();
            selectedSlimeSummonSO = availableSlimeSummonSOList[0];

            OnSlimeSummonListChanged?.Invoke(availableSlimeSummonSOList);
        }

        SlimeSummonCardUI.OnClick += SlimeSummonCardUI_OnClick;
        InputManager.Instance.OnSpawn += InputManager_OnSpawn;
    }

    private void InputManager_OnSpawn(Vector3 worldPosition)
    {
        if (PlayerAI.Instance.IsSummoning)
        {
            Instantiate(selectedSlimeSummonSO.SlimeSummonPrefab, worldPosition, Quaternion.identity);
            PlayerAI.Instance.CancelSummonSlime();

            availableSlimeSummonSOList.Remove(selectedSlimeSummonSO);
            OnSlimeSummonListChanged?.Invoke(availableSlimeSummonSOList);
        }
    }

    private void SlimeSummonCardUI_OnClick(SlimeSummonSO slimeSummonSO)
    {
        PlayerAI.Instance.SummonSlime();
        selectedSlimeSummonSO = slimeSummonSO;
    }

}