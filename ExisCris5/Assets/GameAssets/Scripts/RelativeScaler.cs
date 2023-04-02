using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeScaler : MonoBehaviour
{
    public float Scale = 0.5f;

    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Adjust();
    }

    void Update()
    {
        Adjust();
    }
    
    private void Adjust()
    {
        rectTransform.localScale = Vector3.one * (Scale * Screen.height / rectTransform.rect.height);
    }
}
