using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScr : MonoBehaviour
{
    public float life = 3;
    public float enemiesHitLeft = 5;
    public LayerMask whatIsEnemies;
    public int damage;
    

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == whatIsEnemies)
        {
            collision.collider.GetComponent<EnemyMovement>().TakeDamage(damage);
            enemiesHitLeft--;
        } else {
            Destroy(gameObject);
        }
        
        if (enemiesHitLeft <= 0)
        {
            Destroy(gameObject);
        }
    }
    
}
