using UnityEngine;

public class Creature : MonoBehaviour
{
    public enum Player { Player1, Player2 }

    public Player player;
    public int health;
    public int maxHealth;
    public int attack;
    public int defence;

    protected virtual void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        damage = Mathf.Max(0, damage - defence); // defence reduces damage

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }

}
