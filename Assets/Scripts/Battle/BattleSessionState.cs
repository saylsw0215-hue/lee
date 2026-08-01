using System;

namespace HeroDefense.Battle
{
    /// <summary>Observable, UI-independent state for one battle session.</summary>
    public sealed class BattleSessionState
    {
        public event Action Changed;
        public int CurrentGold { get; private set; }
        public int CurrentWave { get; private set; }
        public int CurrentBaseHp { get; private set; }
        public int MaxBaseHp { get; }
        private readonly int initialGold;
        public BattleSessionState(int gold=500,int maxBaseHp=100){initialGold=Math.Max(0,gold);MaxBaseHp=Math.Max(1,maxBaseHp);CurrentGold=initialGold;CurrentBaseHp=MaxBaseHp;}

        public void AddGold(int amount) { CurrentGold = Math.Max(0, CurrentGold + amount); Changed?.Invoke(); }
        public bool CanAfford(int amount) => amount >= 0 && CurrentGold >= amount;
        public bool TrySpendGold(int amount) { if (!CanAfford(amount)) return false; CurrentGold -= amount; Changed?.Invoke(); return true; }
        public void AdvanceWave() { CurrentWave++; Changed?.Invoke(); }
        public void SetBaseHp(int value) { CurrentBaseHp = Math.Max(0, Math.Min(MaxBaseHp, value)); Changed?.Invoke(); }
        public void DamageBase(int amount) => SetBaseHp(CurrentBaseHp - Math.Max(0, amount));
        public void Reset() { CurrentGold = initialGold; CurrentWave = 0; CurrentBaseHp = MaxBaseHp; Changed?.Invoke(); }
    }
}
