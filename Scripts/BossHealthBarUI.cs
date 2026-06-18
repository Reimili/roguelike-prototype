// ==== BossHealthBarUI.cs ====
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 3f;

    [Header("Colors")]
    [SerializeField] private Color fullColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color lowColor = new Color(0.3f, 0f, 0f);

    private float targetFill = 1f;
    private float maxHealth;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf) return;

        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(
                fillImage.fillAmount,
                targetFill,
                fillSpeed * Time.deltaTime
            );

            fillImage.color = Color.Lerp(lowColor, fullColor, fillImage.fillAmount);
        }
    }

    /// <summary>
    /// Показать полоску здоровья босса
    /// </summary>
    public void Show(string bossName, float maxHP)
    {
        maxHealth = maxHP;

        if (nameText != null)
            nameText.text = bossName;

        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        targetFill = 1f;

        if (panel != null)
            panel.SetActive(true);

        Debug.Log($"[BossHealthBar] Showing for '{bossName}', HP: {maxHP}");
    }

    /// <summary>
    /// Обновить здоровье
    /// </summary>
    public void UpdateHealth(float currentHP)
    {
        if (maxHealth <= 0) return;

        targetFill = Mathf.Clamp01(currentHP / maxHealth);
    }

    /// <summary>
    /// Скрыть полоску
    /// </summary>
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}