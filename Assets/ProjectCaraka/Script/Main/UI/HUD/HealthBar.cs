using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    
    public void SetMaxHealth(float maxHealth)
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
    }

    public void SetHealth(float health)
    {
        healthBar.value = health;
    }
}
