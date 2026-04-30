using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Reference")]
    public HealthBar healthBar;

    [Header("Health System")]
    public float maxHealth = 100f;
    private float currentHealth;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
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
        healthBar.SetHealth(currentHealth);
        CheckDeath();
    }

    public void HealHealth(float health)
    {
        currentHealth += health;
        if (currentHealth >= maxHealth)
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
