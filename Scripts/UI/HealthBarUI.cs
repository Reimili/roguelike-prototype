using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    public void UpdateHealth(float current, float max)
    {
        float percent = max > 0 ? current / max : 0;

        // Обновляем заполнение
        if (fillImage != null)
        {
            fillImage.fillAmount = percent;
            fillImage.color = percent <= lowHealthThreshold ? lowHealthColor : fullHealthColor;
        }

        // Обновляем текст
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(current)} / {Mathf.Ceil(max)}";
        }
    }
}