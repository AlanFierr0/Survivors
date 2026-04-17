using UnityEngine;
public class MeleeEnemy : MonoBehaviour
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; }

    public MeleeEnemy(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }
}