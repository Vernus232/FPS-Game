using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float health;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
            Death();
    }

    private void Death()
    {
        Destroy(gameObject);
    }
}
