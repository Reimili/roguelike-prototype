using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerEffect : MonoBehaviour
{
    public float rotationSpeed = 180f;     // градусов в секунду
    public float growSpeed = 1f;           // скорость увеличени€
    public float maxScale = 1.4f;          // во что вырастет

    private Vector3 startScale;

    private void Start()
    {
        startScale = transform.localScale;
    }

    private void Update()
    {
        // ¬ращение
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // ѕостепенное увеличение
        if (transform.localScale.x < maxScale)
        {
            transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
        }
    }
}

