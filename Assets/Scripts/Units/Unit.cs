using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum Player { Player1, Player2 }

    public Player player;
    public int health;
    public int maxHealth;
    public int attack;
    public int defense;
    public bool isDefending;

    private void Start()
    {
        isDefending = false;
        // TODO: Add a start animation
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        // TODO: Add a death animation
    }

    public void Defend()
    {
        isDefending = true;
    }

    public void EndTurn()
    {
        isDefending = false;
    }
}
