using UnityEngine;

public class Creature : MonoBehaviour
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
