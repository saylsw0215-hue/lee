using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Effects;
using HeroDefense.Battle.Projectiles;
using UnityEngine;

namespace HeroDefense.Battle
{
    /// <summary>Unit-specific expanding pool; combat deaths never destroy objects.</summary>
    public sealed class CombatPool
    {
        private readonly Transform parent;
        private readonly CombatRegistry registry;
        private readonly FloatingDamageTextPool texts;
        private readonly ProjectilePool projectiles;
        private readonly Dictionary<string, Queue<CombatUnit>> queues = new();
        private readonly List<CombatUnit> active = new(40);
        public IReadOnlyList<CombatUnit> Active => active;

        public CombatPool(Transform parent, CombatRegistry registry, FloatingDamageTextPool texts,ProjectilePool projectilePool=null)
        { this.parent = parent; this.registry = registry; this.texts = texts;projectiles=projectilePool; }

        public void Prewarm(UnitData data, int count)
        {
            EnsureQueue(data.UnitId);
            for (int i = 0; i < count; i++) queues[data.UnitId].Enqueue(Create(data));
        }
        public CombatUnit Spawn(UnitData data, Vector2 localPosition, float forwardLimit)
        {
            EnsureQueue(data.UnitId); CombatUnit unit = queues[data.UnitId].Count > 0 ? queues[data.UnitId].Dequeue() : Create(data);
            if (!active.Contains(unit)) active.Add(unit); unit.Spawn(localPosition, forwardLimit); return unit;
        }
        public void Return(CombatUnit unit)
        {
            if (unit == null || !active.Remove(unit)) return;
            unit.MarkPooled(); EnsureQueue(unit.Data.UnitId); queues[unit.Data.UnitId].Enqueue(unit);
        }
        public void ReturnAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                CombatUnit unit = active[i]; unit.ReturnWithoutReward(); EnsureQueue(unit.Data.UnitId); queues[unit.Data.UnitId].Enqueue(unit);
            }
            active.Clear();
            texts?.ReturnAll();
            projectiles?.ReturnAll();
        }
        private CombatUnit Create(UnitData data)
        {
            var go = new GameObject($"Pooled_{data.UnitId}", typeof(RectTransform), typeof(HealthComponent), typeof(UnitVisualController), typeof(CombatUnit));
            go.transform.SetParent(parent, false); CombatUnit unit = go.GetComponent<CombatUnit>(); unit.Construct(data, registry, this, texts,projectiles); return unit;
        }
        private void EnsureQueue(string id) { if (!queues.ContainsKey(id)) queues.Add(id, new Queue<CombatUnit>()); }
    }
}
