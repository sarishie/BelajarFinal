using System.Collections;
using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("Reference")]
    public SpriteRenderer spriteRenderer;

    [Header("Hit Effect")]
    public Color hitColor = Color.red;
    public float blinkInterval = 0.1f;
    public int blinkCount = 3;

    private Color originalColor;
    private Coroutine hitEffectCoroutine;

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void PlayHitEffect()
    {
        if (spriteRenderer == null) return;

        if (hitEffectCoroutine != null)
        {
            StopCoroutine(hitEffectCoroutine);
        }

        hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(blinkInterval);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.color = originalColor;
    }
}