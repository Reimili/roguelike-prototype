using UnityEngine;

public abstract class ItemEffectSO : ScriptableObject
{
    public string effectName;
    [TextArea]
    public string effectDescription;

    ///Когда предмет подобран
    public virtual void OnPickup(GameObject player) { }

    ///Когда предмет убран из инвентаря
    public virtual void OnRemove(GameObject player) { }

    ///Каждый кадр
    public virtual void OnUpdate(GameObject player) { }

    ///Когда игрок наносит урон
    public virtual void OnDealDamage(GameObject player, GameObject enemy, ref float damage) { }

    ///Когда игрок получает урон
    public virtual void OnTakeDamage(GameObject player, ref float damage) { }

    ///Когда враг убит
    public virtual void OnKillEnemy(GameObject player, GameObject enemy) { }
}