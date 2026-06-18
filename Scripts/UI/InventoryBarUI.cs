// ==== InventoryBarUI.cs ====
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemSlotPrefab;


    private Dictionary<ItemDataSO, GameObject> itemSlots = new Dictionary<ItemDataSO, GameObject>();

    public void AddItem(ItemDataSO item)
    {
        if (item == null) return;

        // Если уже есть — обновляем счётчик (для стакающихся)
        if (itemSlots.ContainsKey(item))
        {
            UpdateItemCount(item);
            return;
        }

        // Создаём новый слот
        GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);
        ItemSlotUI slotUI = slot.GetComponent<ItemSlotUI>();

        if (slotUI != null)
        {
            slotUI.Setup(item);
        }

        itemSlots[item] = slot;
    }

    public void RemoveItem(ItemDataSO item)
    {
        if (item == null) return;

        if (itemSlots.TryGetValue(item, out GameObject slot))
        {
            Destroy(slot);
            itemSlots.Remove(item);
        }
    }

    private void UpdateItemCount(ItemDataSO item)
    {
        if (itemSlots.TryGetValue(item, out GameObject slot))
        {
            ItemSlotUI slotUI = slot.GetComponent<ItemSlotUI>();
            if (slotUI != null)
            {
                int count = Inventory.Instance.CountItem(item);
                slotUI.UpdateCount(count);
            }
        }
    }

    public void Clear()
    {
        foreach (var slot in itemSlots.Values)
        {
            Destroy(slot);
        }
        itemSlots.Clear();
    }
}