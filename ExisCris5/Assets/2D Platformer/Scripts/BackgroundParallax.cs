using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    public Camera Camera;
    public float ParallaxScale = 1;

    private Vector3 originalPosition;
    private Vector3 center;
    
    void Start()
    {
        if (!Camera)
            Camera = Camera.main;

        center = Camera.transform.position;
        originalPosition = transform.position;
    }

    void Update()
    {
        var delta = Camera.transform.position - center;
        transform.position = originalPosition - delta * ParallaxScale;
    }
}
