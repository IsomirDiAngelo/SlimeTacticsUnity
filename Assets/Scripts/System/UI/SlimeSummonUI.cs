using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlimeSummonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SlimeSummonCardUI slimeSummonCardPrefab;
    [SerializeField] private Transform backgroundRef;
    private List<SlimeSummonCardUI> slimeSummonCards;

    private void Start()
    {
        slimeSummonCards = new();
        SlimeSummonerManager.Instance.OnSlimeSummonListChanged += SlimeSummonerManager_OnSlimeSummonListChanged;
    }

    private void SlimeSummonerManager_OnSlimeSummonListChanged(List<SlimeSummonSO> slimeSummonSOList)
    {
        if (slimeSummonCards != null)
        {
            foreach (SlimeSummonCardUI card in slimeSummonCards)
            {
                Destroy(card.gameObject);
            }
        }

        slimeSummonCards = new();
        foreach (SlimeSummonSO slimeSummonSO in slimeSummonSOList)
        {
            SlimeSummonCardUI card = Instantiate(slimeSummonCardPrefab, backgroundRef);
            card.SetSlimeSummonSO(slimeSummonSO);
            slimeSummonCards.Add(card);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        InputManager.Instance.SetHoveringUI(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InputManager.Instance.SetHoveringUI(false);
    }
}
