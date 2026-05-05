using System.Collections;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Reference")]
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    [Header("State")]
    public bool isKnockbacking;

    private Coroutine knockbackCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void Knockback(Vector2 direction, float force, float duration)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        isKnockbacking = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        rb.velocity = Vector2.zero;
        rb.velocity = direction.normalized * force;

        yield return new WaitForSeconds(duration);

        isKnockbacking = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }
}