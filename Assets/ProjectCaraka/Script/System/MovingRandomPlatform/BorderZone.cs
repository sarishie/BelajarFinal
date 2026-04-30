using UnityEngine;

public class BorderZone : MonoBehaviour
{
    [Header("Settings")]
    public bool killPlayer = true;
    public bool destroyPlatform = true;

    [Header("Damage")]
    public float borderDamage = 9999f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (killPlayer)
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(borderDamage);
                return;
            }
        }

        if (destroyPlatform)
        {
            MovingPlatform platform = collision.GetComponentInParent<MovingPlatform>();

            if (platform != null)
            {
                Destroy(platform.gameObject);
            }
        }
    }
}