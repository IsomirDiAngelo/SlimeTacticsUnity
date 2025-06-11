using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour
{
    private List<Renderer> renderers;
    private bool isFlashing = false;
    private float flashTimer = 0f;
    private SlimeCombat slimeCombat;

    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 1f;
    [SerializeField] private float flashIntensity = 5f;

    private void Start()
    {
        renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        if (TryGetComponent(out slimeCombat))
        {
            slimeCombat.OnHitTaken += SlimeCombat_OnHitTaken;
        }
    }

    private void SlimeCombat_OnHitTaken()
    {
        FlashObject();
    }

    private void Update()
    {
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;

            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    renderer.materials[i].EnableKeyword("_EMISSION");
                    renderer.materials[i].SetColor("_EmissionColor", flashColor * (1 - flashTimer / flashDuration) * flashIntensity);
                }
            }

            if (flashTimer >= flashDuration) 
            {
                isFlashing = false;
                flashTimer = 0f;

                foreach (var renderer in renderers)
                {
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        renderer.materials[i].DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }

    public void FlashObject()
    {
        if (!isFlashing)
        {
            isFlashing = true;
        }

        flashTimer = 0f;
    }
}
