using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float DestructionDelay = 0.125f;
    public GameObject ExplosionTemplate;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
        }

        if (ExplosionTemplate)
        {
            var explosion = Instantiate(ExplosionTemplate);
            explosion.SetActive(true);
            explosion.transform.position = transform.position;
        }
        
        Destroy(gameObject, DestructionDelay);
    }
}
