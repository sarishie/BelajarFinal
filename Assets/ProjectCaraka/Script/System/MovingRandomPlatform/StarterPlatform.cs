using System.Collections;
using System.Threading;
using UnityEngine;

public class StarterPlatform : MonoBehaviour
{
    [Header("Life Time")]
    public float lifeTime = 10f;

    [Header("Blink Warning")]
    public bool blinkBeforeDestroy = true;
    public float blinkStartTime = 3f;
    public float blinkInterval = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        StartCoroutine(StarterPlatformRoutine());
    }

    private IEnumerator StarterPlatformRoutine()
    {
        float timer = lifeTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Debug.Log(timer);
            if (blinkBeforeDestroy && timer < blinkStartTime && spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkInterval);
            }
            else
            {
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
}