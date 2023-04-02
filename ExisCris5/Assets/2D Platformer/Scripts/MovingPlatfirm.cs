using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatfirm : MonoBehaviour
{
    public float Speed = 1;
    public List<Transform> Waypoints;

    private float progress;

    void Start()
    {
    }

    void Update()
    {
        if (Waypoints.Count == 0)
            return;

        var pointA = Waypoints[0].position;
        var pointB = Waypoints[1].position;
        progress = (Time.time * Speed) % 1f;
        progress = 1f - Mathf.Abs(progress * 2 - 1);
        transform.position = Vector3.Lerp(pointA, pointB, progress);
    }
}