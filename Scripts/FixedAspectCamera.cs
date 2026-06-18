using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    [Header("Target Aspect")]
    [SerializeField] private float targetWidth = 16f;
    [SerializeField] private float targetHeight = 9f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdateAspect();
    }

    private void Update()
    {
        // Если окно меняет размер — пересчитываем
        UpdateAspect();
    }

    private void UpdateAspect()
    {
        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Экран уже, чем нужно → полосы сверху/снизу
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            // Экран шире, чем нужно → полосы по бокам
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}