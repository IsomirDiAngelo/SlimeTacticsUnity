using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlimeSummonCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI slimeNameText;
    [SerializeField] private Image cardBackgroundImage;
    private SlimeSummonSO slimeSummonSO;
    private readonly Color BACKGROUND_COLOR = new(0.2f, 0.84f, 0.42f, 0.05f);
    private readonly Color BACKGROUND_COLOR_HOVERED = new(0.8f, 0.84f, 0.42f, 0.2f);

    public static event Action<SlimeSummonSO> OnClick;


    public void SetSlimeSummonSO(SlimeSummonSO newSlimeSummonSO)
    {
        slimeSummonSO = newSlimeSummonSO;
        SetSlimeName(slimeSummonSO.SlimeName);
        SetSlimeSprite();
    }

    private void SetSlimeName(string slimeName)
    {
        slimeNameText.text = slimeName;
    }

    private void SetSlimeSprite()
    {
        // TODO
    }

    private void SetHovered(bool hovered)
    {
        Color cardBackgroundColor = hovered ? BACKGROUND_COLOR_HOVERED : BACKGROUND_COLOR;
        cardBackgroundImage.color = cardBackgroundColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(slimeSummonSO);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }
}
