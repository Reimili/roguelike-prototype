// ==== ItemSlotUI.cs ====
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image borderImage;      // Корневой ItemSlot (рамка)
    [SerializeField] private Image backgroundImage;  // Тёмный фон внутри
    [SerializeField] private Image iconImage;        // Иконка предмета
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.9f);

    [Header("Rarity Border Colors")]
    [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.6f);      // Серый
    [SerializeField] private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f);    // Зелёный
    [SerializeField] private Color rareColor = new Color(0.3f, 0.5f, 1f);          // Синий
    [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0.2f);     // Золотой

    private ItemDataSO itemData;

    void Awake()
    {
        // Если borderImage не назначен, используем свой Image
        if (borderImage == null)
            borderImage = GetComponent<Image>();
    }

    public void Setup(ItemDataSO item)
    {
        itemData = item;

        // Рамка — цвет по редкости
        if (borderImage != null)
        {
            borderImage.color = GetRarityColor(item.rarity);
        }

        // Фон — тёмный
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        // Иконка — БЕЗ изменения цвета!
        if (iconImage != null)
        {
            if (item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.color = Color.white;  // ВАЖНО!
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        // Счётчик
        UpdateCount(1);
    }

    public void UpdateCount(int count)
    {
        if (countText != null)
        {
            if (count > 1)
            {
                countText.text = count.ToString();
                countText.enabled = true;
            }
            else
            {
                countText.text = "";
                countText.enabled = false;
            }
        }
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonColor,
            Rarity.Uncommon => uncommonColor,
            Rarity.Rare => rareColor,
            Rarity.Legendary => legendaryColor,
            _ => commonColor
        };
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Show(itemData, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Hide();
        }
    }
}