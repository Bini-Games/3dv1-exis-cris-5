using System.Collections;
using System.Collections.Generic;
using Platformer;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float TotalDuration = 2f;
    public float DestructionDelay = 0.125f;
    public GameObject ExplosionTemplate;

    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time > startTime + TotalDuration)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var destructableEntity = other.gameObject.GetComponent<DestructableEntity>();
        
        if (destructableEntity)
            destructableEntity.Die();

        if (ExplosionTemplate)
        {
            var explosion = Instantiate(ExplosionTemplate);
            explosion.SetActive(true);
            explosion.transform.position = transform.position;
        }
        
        Destroy(gameObject, DestructionDelay);
    }
}
