using System.Collections;
using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    [Header("Knockback")]
    public Vector2 knockbackDirection = Vector2.up;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.25f;

    [Header("Cooldown")]
    public float damageCooldown = 0.5f;
    private bool canDamage = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDamage) return;

        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        PlayerKnockback playerKnockback = collision.GetComponentInParent<PlayerKnockback>();

        if (playerHealth == null) return;

        playerHealth.TakeDamage(damage);

        if (playerKnockback != null)
        {
            playerKnockback.Knockback(knockbackDirection, knockbackForce, knockbackDuration);
        }

        StartCoroutine(DamageCooldownRoutine());
    }

    private IEnumerator DamageCooldownRoutine()
    {
        canDamage = false;

        yield return new WaitForSeconds(damageCooldown);

        canDamage = true;
    }
}