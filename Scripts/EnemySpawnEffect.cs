using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnEffect : MonoBehaviour
{
    public float stretchY = 2.5f;      // Насколько растянуть по вертикали
    public float normalizeSpeed = 8f;  // Скорость возвращения к нормальному размеру

    private Vector3 targetScale;

    private void Start()
    {
        targetScale = transform.localScale;

        // Начальное "телепортированное" состояние
        transform.localScale = new Vector3(
            targetScale.x,
            targetScale.y * stretchY,
            targetScale.z
        );
    }

    private void Update()
    {
        // Плавное схлопывание в нормальную форму
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * normalizeSpeed
        );
    }
}

