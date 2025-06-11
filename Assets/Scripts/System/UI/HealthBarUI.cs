using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private SlimeCombat slimeCombat;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image healthBarDamageImage;
    [SerializeField] private float inactivityTime = 10f;
    [SerializeField] private float animationSpeed = 10f;

    private bool isAnimating;
    private float targetHealthNormalized;

    private const float FILL_AMOUNT_ERROR_MARGIN = 0.001f;

    private void Start()
    {
        slimeCombat.CombatStats.OnHealthChange += SlimeCombat_OnHealthChange;

        Hide();
    }

    private void Update()
    {
        if (isAnimating)
        {
            if (Mathf.Abs(healthBarDamageImage.fillAmount - targetHealthNormalized) > FILL_AMOUNT_ERROR_MARGIN)
            {
                healthBarDamageImage.fillAmount = Mathf.Lerp(healthBarDamageImage.fillAmount, targetHealthNormalized, animationSpeed * Time.deltaTime);

                if (healthBarDamageImage.fillAmount <= FILL_AMOUNT_ERROR_MARGIN)
                {
                    StopCoroutine(nameof(HideAfterInactivty));
                    Hide();
                }
            }
            else
            {
                healthBarDamageImage.fillAmount = healthBarImage.fillAmount; // Prevent small inconsistencies
                isAnimating = false;
            }
        }
    }

    private void SlimeCombat_OnHealthChange(float newHealthAmount)
    {
        targetHealthNormalized = newHealthAmount / slimeCombat.CombatStats.MaxHealth;
        healthBarImage.fillAmount = targetHealthNormalized;

        RefreshActivity();

        isAnimating = true;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void RefreshActivity()
    {
        Show();
        StopCoroutine(nameof(HideAfterInactivty));
        StartCoroutine(nameof(HideAfterInactivty));
    }

    private IEnumerator HideAfterInactivty()
    {
        yield return new WaitForSeconds(inactivityTime);
        Hide();
    }
}
