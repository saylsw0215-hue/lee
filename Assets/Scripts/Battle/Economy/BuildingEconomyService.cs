using HeroDefense.Battle.Buildings;
using HeroDefense.Meta;
using UnityEngine;

namespace HeroDefense.Battle.Economy
{
    /// <summary>Centralizes construction, upgrade, and one-time sale transactions.</summary>
    public sealed class BuildingEconomyService
    {
        private readonly BattleSessionState session;
        public BuildingEconomyService(BattleSessionState state){session=state;}
        public bool TryPayBuild(BuildingData data)=>data!=null&&session.TrySpendGold(Mathf.CeilToInt(data.BuildCost*MetaRuntimeModifierProvider.BuildCostMultiplier));
        public bool TryUpgrade(BuildingRuntimeState building)
        {
            if(building==null||building.IsMaxLevel||building.IsConstructing||building.IsSold)return false;
            int cost=Mathf.CeilToInt(building.Data.GetUpgradeCost(building.CurrentLevel)*MetaRuntimeModifierProvider.UpgradeCostMultiplier); if(!session.TrySpendGold(cost))return false;
            return building.TryUpgrade();
        }
        public int TrySell(BuildingRuntimeState building)
        {
            if(building==null||!building.TryMarkSold())return 0; int value=building.Data.CalculateSellValue(building.CurrentLevel);session.AddGold(value);return value;
        }
    }
}
