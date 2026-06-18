// ==== CheatManager.cs ====
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    void Update()
    {
        // G Ч случайный предмет
        if (Input.GetKeyDown(KeyCode.G))
        {
            GiveRandomItem();
        }

        // H Ч все предметы
        if (Input.GetKeyDown(KeyCode.H))
        {
            GiveAllItems();
        }

        // J Ч полное здоровье
        if (Input.GetKeyDown(KeyCode.J))
        {
            FullHeal();
        }

        // K Ч получить урон (дл€ теста)
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage();
        }
    }

    void GiveRandomItem()
    {
        if (ItemDatabase.Instance == null || Inventory.Instance == null) return;

        var item = ItemDatabase.Instance.GetRandomItem(ItemPool.Treasure, false);
        if (item != null)
        {
            Inventory.Instance.TryAddItem(item);
            Debug.Log($"[CHEAT] Got: {item.itemName}");
        }
    }

    void GiveAllItems()
    {
        if (ItemDatabase.Instance == null || Inventory.Instance == null) return;

        var all = ItemDatabase.Instance.GetAvailableItems(ItemPool.Any, false);
        foreach (var entry in all)
        {
            if (entry.item != null)
                Inventory.Instance.TryAddItem(entry.item);
        }
        Debug.Log($"[CHEAT] Got {all.Count} items!");
    }

    void FullHeal()
    {
        if (PlayerHealth.Instance == null) return;
        PlayerHealth.Instance.Heal(999);
        Debug.Log("[CHEAT] Full heal!");
    }

    void TakeDamage()
    {
        if (PlayerHealth.Instance == null) return;
        PlayerHealth.Instance.TakeDamage(1);
        Debug.Log("[CHEAT] Took 1 damage");
    }
}