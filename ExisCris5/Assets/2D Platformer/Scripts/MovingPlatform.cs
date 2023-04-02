using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float Speed = 1;
    public List<Transform> Waypoints;

    private float progress;

    private Rigidbody2D rigidbody;
    
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Waypoints.Count == 0)
            return;

        var pointA = Waypoints[0].position;
        var pointB = Waypoints[1].position;
        progress = (Time.time * Speed) % 1f;
        progress = 1f - Mathf.Abs(progress * 2 - 1);
        var oldPosition = transform.position;
        var newPosition = Vector3.Lerp(pointA, pointB, progress);
        rigidbody.velocity = newPosition - oldPosition;
    }
}