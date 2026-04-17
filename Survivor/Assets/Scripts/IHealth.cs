public interface IHealth
{
    int MaxHealth { get; }
    int CurrentHealth { get; }
    void TakeDamage(int damage);
    void Heal(int amount);
    int GetCurrentHealth();
    int GetMaxHealth();
}

