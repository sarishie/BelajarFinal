using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Reference")]
    public HealthBar healthBar;
    private PlayerHitEffect playerHitEffect;

    [Header("Health System")]
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        playerHitEffect = GetComponent<PlayerHitEffect>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            TakeDamage(20);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            HealHealth(20);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        healthBar.SetHealth(currentHealth);

        if (playerHitEffect != null)
        {
            playerHitEffect.PlayHitEffect();
        }

        CheckDeath();
    }

    public void HealHealth(float health)
    {
        currentHealth += health;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthBar.SetHealth(currentHealth);
        CheckDeath();
    }

    public void CheckDeath()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}