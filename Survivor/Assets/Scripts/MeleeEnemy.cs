public class MeleeEnemy : IHealth
{
    public int CurrentHealth { get; private set; }

    public MeleeEnemy(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0)
            CurrentHealth = 0;
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
    }

    public int GetCurrentHealth()
    {
        return CurrentHealth;
    }

    public int GetMaxHealth()
    {
        return MaxHealth;
    }
}